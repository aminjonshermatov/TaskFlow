using Grpc.Core;
using TaskFlow.Contracts.Grpc.V1;
using TaskFlow.Coordinator.Workers;

namespace TaskFlow.Coordinator.Services;

public class WorkerControlService(
    IWorkerRegistry workerRegistry,
    TimeProvider timeProvider,
    ILogger<WorkerControlService> logger) : WorkerControl.WorkerControlBase
{
    public override async Task Connect(
        IAsyncStreamReader<WorkerEvent> requestStream,
        IServerStreamWriter<CoordinatorEvent> responseStream,
        ServerCallContext context)
    {
        WorkerSession? session = null;
        Task? writeTask = null;
        try
        {
            logger.LogInformation("Worker stream connected.");

            while (await requestStream.MoveNext(context.CancellationToken))
            {
                var message = requestStream.Current;

                switch (message.PayloadCase)
                {
                    case WorkerEvent.PayloadOneofCase.Hello:
                        if (session is not null)
                        {
                            throw new RpcException(new Status(
                                StatusCode.FailedPrecondition,
                                "Worker already sent Hello for this connection."));
                        }
                        var hello = message.Hello;

                        var candidateSession = new WorkerSession
                        {
                            Id = Guid.NewGuid(),
                            WorkerInfo = new WorkerInfo(hello.WorkerId, hello.WorkerName),
                            ConnectedAt = timeProvider.GetUtcNow()
                        };
                        await candidateSession.EnqueueAsync(
                            new CoordinatorEvent
                            {
                                Welcome = new CoordinatorWelcome
                                {
                                    CoordinatorId = "coordinator_1"
                                }
                            },
                            context.CancellationToken);

                        if (!workerRegistry.Register(candidateSession))
                        {
                            candidateSession.CompleteOutgoing();
                            throw new RpcException(new Status(
                                StatusCode.AlreadyExists,
                                $"Could not register worker '{hello.WorkerId}'."));
                        }
                        session = candidateSession;
                        writeTask = WriteOutgoingAsync(session, responseStream, context.CancellationToken);
                        logger.LogInformation(
                            "Worker registered: {WorkerId} ({WorkerName}), session {SessionId}",
                            hello.WorkerId,
                            hello.WorkerName,
                            session.Id);
                        break;
                    case WorkerEvent.PayloadOneofCase.Heartbeat:
                        if (session is null)
                        {
                            throw new RpcException(
                                new Status(StatusCode.FailedPrecondition, "Worker must send Hello before Heartbeat."));
                        }
                        logger.LogDebug("Heartbeat received from worker {WorkerId}.", session.WorkerInfo.Id);
                        break;
                    case WorkerEvent.PayloadOneofCase.None:
                        break;
                }
            }
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker stream was cancelled.");
        }
        catch (IOException)
        {
            logger.LogInformation("Worker stream was disconnected.");
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            logger.LogInformation("Worker stream was cancelled by client.");
        }
        finally
        {
            if (session is not null)
            {
                var removed = workerRegistry.Unregister(session.WorkerInfo.Id, session.Id);
                session.CompleteOutgoing();

                logger.LogInformation(
                    "Worker unregistered: {WorkerId}, session {SessionId}, removed: {Removed}",
                    session.WorkerInfo.Id,
                    session.Id,
                    removed);
            }
            if (writeTask is not null)
            {
                try
                {
                    await writeTask;
                }
                catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
                {
                }
                catch (IOException exception)
                {
                    logger.LogDebug(exception, "Worker response stream closed.");
                }
            }
        }
    }
    
    private static async Task WriteOutgoingAsync(
        WorkerSession session,
        IServerStreamWriter<CoordinatorEvent> responseStream,
        CancellationToken cancellationToken)
    {
        await foreach (var @event in session.ReadOutgoingAsync(cancellationToken))
        {
            await responseStream.WriteAsync(@event, cancellationToken);
        }
    }
}
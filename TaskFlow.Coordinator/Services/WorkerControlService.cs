using Grpc.Core;
using TaskFlow.Contracts.Grpc.V1;
using TaskFlow.Coordinator.Workers;

namespace TaskFlow.Coordinator.Services;

public class WorkerControlService(
    IWorkerRegistry workerRegistry,
    ILogger<WorkerControlService> logger) : WorkerControl.WorkerControlBase
{
    public override async Task Connect(
        IAsyncStreamReader<WorkerEvent> requestStream,
        IServerStreamWriter<CoordinatorEvent> responseStream,
        ServerCallContext context)
    {
        WorkerSession? session = null;
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
                            ConnectedAt = DateTime.UtcNow
                        };
                        if (!workerRegistry.Register(candidateSession))
                        {
                            throw new RpcException(new Status(
                                StatusCode.AlreadyExists,
                                $"Could not register worker '{hello.WorkerId}'."));
                        }
                        session = candidateSession;
                        logger.LogInformation(
                            "Worker registered: {WorkerId} ({WorkerName}), session {SessionId}",
                            hello.WorkerId,
                            hello.WorkerName,
                            session.Id);

                        await responseStream.WriteAsync(new CoordinatorEvent
                        {
                            Welcome = new CoordinatorWelcome
                            {
                                CoordinatorId = "coordinator_1"
                            }
                        });
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
        finally
        {
            if (session is not null)
            {
                var removed = workerRegistry.Unregister(
                    session.WorkerInfo.Id,
                    session.Id);

                logger.LogInformation(
                    "Worker unregistered: {WorkerId}, session {SessionId}, removed: {Removed}",
                    session.WorkerInfo.Id,
                    session.Id,
                    removed);
            }
        }
    }
}
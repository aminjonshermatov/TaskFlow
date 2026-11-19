using Grpc.Core;

using TaskFlow.Contracts.Grpc.V1;
using TaskFlow.Coordinator.Workers;

namespace TaskFlow.Coordinator.Services;

public partial class WorkerControlService(
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
      LogStreamConnected();

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

            var now = timeProvider.GetUtcNow();
            var candidateSession = new WorkerSession
            {
              Id = Guid.CreateVersion7(now),
              WorkerInfo = new WorkerInfo(hello.WorkerId, hello.WorkerName),
              ConnectedAt = now
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
            LogWorkerRegistered(hello.WorkerId, hello.WorkerName, session.Id);
            break;
          case WorkerEvent.PayloadOneofCase.Heartbeat:
            if (session is null)
            {
              throw new RpcException(
                  new Status(StatusCode.FailedPrecondition, "Worker must send Hello before Heartbeat."));
            }
            LogHeartbeatReceived(session.WorkerInfo.Id);
            break;
          case WorkerEvent.PayloadOneofCase.None:
            break;
        }
      }
    }
    catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
    {
      LogStreamCancelled();
    }
    catch (IOException)
    {
      LogStreamDisconnected();
    }
    catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
    {
      LogStreamCancelledByClient();
    }
    finally
    {
      if (session is not null)
      {
        var removed = workerRegistry.Unregister(session.WorkerInfo.Id, session.Id);
        session.CompleteOutgoing();

        LogWorkerUnregistered(session.WorkerInfo.Id, session.Id, removed);
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
          LogResponseStreamClosed(exception);
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

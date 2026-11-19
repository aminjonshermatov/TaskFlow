using Grpc.Core;
using Grpc.Net.Client;

using TaskFlow.Contracts.Grpc.V1;

namespace TaskFlow.Worker.Protocol;

public sealed partial class CoordinatorConnection(
    ILogger<CoordinatorConnection> logger)
{
  public async Task RunAsync(
      string coordinatorUrl,
      WorkerIdentity worker,
      CancellationToken stoppingToken)
  {
    using var channel = GrpcChannel.ForAddress(coordinatorUrl);
    var client = new WorkerControl.WorkerControlClient(channel);
    using var call = client.Connect(cancellationToken: stoppingToken);

    var sender = new WorkerEventSender();
    var sendTask = sender.PumpAsync(call.RequestStream, stoppingToken);

    await sender.SendAsync(
        new WorkerEvent
        {
          Hello = new WorkerHello
          {
            WorkerId = worker.Id,
            WorkerName = worker.Name
          }
        },
        stoppingToken);

    LogHelloSent();

    try
    {
      while (await call.ResponseStream.MoveNext(stoppingToken))
      {
        var message = call.ResponseStream.Current;
        switch (message.PayloadCase)
        {
          case CoordinatorEvent.PayloadOneofCase.Welcome:
            LogConnected(message.Welcome.CoordinatorId);
            break;
          case CoordinatorEvent.PayloadOneofCase.JobAssignment:
            LogJobAssigned(message.JobAssignment.JobId);
            break;
        }
      }
      LogStreamEnded();
    }
    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
    {
      LogStopping();
    }
    catch (RpcException exception) when (exception.StatusCode == StatusCode.Cancelled && stoppingToken.IsCancellationRequested)
    {
      LogCallCancelled();
    }
    finally
    {
      sender.Complete();
      try
      {
        await sendTask;
      }
      catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
      {
      }

      try
      {
        await call.RequestStream.CompleteAsync();
      }
      catch (RpcException) when (stoppingToken.IsCancellationRequested)
      {
      }
    }
  }
}

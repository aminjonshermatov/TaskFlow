using Grpc.Core;
using Grpc.Net.Client;
using TaskFlow.Contracts.Grpc.V1;

namespace TaskFlow.Worker.Protocol;

public sealed class CoordinatorConnection(
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

        logger.LogInformation("Hello sent. Waiting for coordinator.");

        try
        {
            while (await call.ResponseStream.MoveNext(stoppingToken))
            {
                var message = call.ResponseStream.Current;
                switch (message.PayloadCase)
                {
                    case CoordinatorEvent.PayloadOneofCase.Welcome:
                        logger.LogInformation("Connected to coordinator {CoordinatorId}.", message.Welcome.CoordinatorId);
                        break;
                    case CoordinatorEvent.PayloadOneofCase.JobAssignment:
                        logger.LogInformation("Received assignment for job {JobId}.", message.JobAssignment.JobId);
                        break;
                }
            }
            logger.LogWarning("Coordinator stream ended.");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker is stopping.");
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.Cancelled && stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker call was cancelled.");
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
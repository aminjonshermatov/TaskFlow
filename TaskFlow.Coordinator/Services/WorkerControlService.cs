using Grpc.Core;
using TaskFlow.Contracts.Grpc.V1;

namespace TaskScheduler.Coordinator.Services;

public class WorkerControlService(ILogger<WorkerControlService> logger) : WorkerControl.WorkerControlBase
{
    public override async Task Connect(
        IAsyncStreamReader<WorkerEvent> requestStream,
        IServerStreamWriter<CoordinatorEvent> responseStream,
        ServerCallContext context)
    {
        logger.LogInformation("Worker stream connected.");

        while (await requestStream.MoveNext(context.CancellationToken))
        {
            var message = requestStream.Current;

            switch (message.PayloadCase)
            {
                case WorkerEvent.PayloadOneofCase.Hello:
                    var hello = message.Hello;
                    logger.LogInformation($"Worker registered: {hello.WorkerId} {hello.WorkerName}");
                    await responseStream.WriteAsync(new CoordinatorEvent
                    {
                        Welcome = new CoordinatorWelcome
                        {
                            CoordinatorId = "coordinator_1"
                        }
                    });
                    break;
                case WorkerEvent.PayloadOneofCase.Heartbeat:
                    logger.LogDebug("Heartbeat received.");
                    break;
                case WorkerEvent.PayloadOneofCase.None:
                    break;
            }
        }
        logger.LogInformation("Worker stream disconnected.");
    }
}
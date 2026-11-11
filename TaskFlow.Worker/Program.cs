using Grpc.Net.Client;
using TaskFlow.Contracts.Grpc.V1;

var coordinatorUrl = Environment.GetEnvironmentVariable("TASKFLOW_COORDINATOR_URL") ?? "";

using var channel = GrpcChannel.ForAddress(coordinatorUrl);

var client = new WorkerControl.WorkerControlClient(channel);
using var call = client.Connect();

await call.RequestStream.WriteAsync(new WorkerEvent
{
    Hello = new WorkerHello
    {
        WorkerId = Guid.NewGuid().ToString(),
        WorkerName = Environment.MachineName
    }
});

Console.WriteLine("Hello send. Waiting for coordinator");

var receiveTask = Task.Run(async () =>
{
    while (await call.ResponseStream.MoveNext(CancellationToken.None))
    {
        var message = call.ResponseStream.Current;
        if (message.PayloadCase == CoordinatorEvent.PayloadOneofCase.Welcome)
        {
            Console.WriteLine($"Connected to {message.Welcome.CoordinatorId}");
        }
    }
});

using var shutdown = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    shutdown.Cancel();
};

try
{
    await Task.Delay(Timeout.InfiniteTimeSpan, shutdown.Token);
}
catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
{
    Console.WriteLine("Stopping worker...");
}
finally
{
    await call.RequestStream.CompleteAsync();
}

await receiveTask;
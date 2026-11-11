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

while (await call.ResponseStream.MoveNext(CancellationToken.None))
{
    var message = call.ResponseStream.Current;
    if (message.PayloadCase == CoordinatorEvent.PayloadOneofCase.Welcome)
    {
        Console.WriteLine($"Connected to {message.Welcome.CoordinatorId}");
    }
}
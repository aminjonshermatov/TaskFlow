using System.Threading.Channels;
using TaskFlow.Contracts.Grpc.V1;

namespace TaskFlow.Coordinator.Workers;

public sealed class WorkerSession
{
    private readonly Channel<CoordinatorEvent> _outgoing =
        Channel.CreateBounded<CoordinatorEvent>(
            new BoundedChannelOptions(32)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            });

    public Guid Id { get; init; }
    public required WorkerInfo WorkerInfo { get; init; }
    public DateTimeOffset ConnectedAt { get; init; }
    
    public ValueTask EnqueueAsync(CoordinatorEvent @event, CancellationToken cancellationToken) =>
        _outgoing.Writer.WriteAsync(@event, cancellationToken);

    public IAsyncEnumerable<CoordinatorEvent> ReadOutgoingAsync(CancellationToken cancellationToken) =>
        _outgoing.Reader.ReadAllAsync(cancellationToken);

    public void CompleteOutgoing() => _outgoing.Writer.TryComplete();

}
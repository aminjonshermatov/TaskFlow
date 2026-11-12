using System.Threading.Channels;
using TaskFlow.Contracts.Grpc.V1;

namespace TaskFlow.Coordinator.Workers;

public sealed class WorkerSession
{
    public Guid Id { get; init; }

    public required WorkerInfo WorkerInfo { get; init; }

    public DateTimeOffset ConnectedAt { get; init; }

    public Channel<CoordinatorEvent> Outbound { get; } =
        Channel.CreateUnbounded<CoordinatorEvent>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
}

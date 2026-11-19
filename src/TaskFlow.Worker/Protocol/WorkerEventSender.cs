using System.Threading.Channels;

using Grpc.Core;

using TaskFlow.Contracts.Grpc.V1;

namespace TaskFlow.Worker.Protocol;

public sealed class WorkerEventSender
{
  private readonly Channel<WorkerEvent> _events =
      Channel.CreateBounded<WorkerEvent>(
          new BoundedChannelOptions(32)
          {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
          });

  public ValueTask SendAsync(
      WorkerEvent @event,
      CancellationToken cancellationToken) =>
      _events.Writer.WriteAsync(@event, cancellationToken);

  public async Task PumpAsync(
      IClientStreamWriter<WorkerEvent> requestStream,
      CancellationToken cancellationToken)
  {
    await foreach (var @event in _events.Reader.ReadAllAsync(cancellationToken))
    {
      await requestStream.WriteAsync(@event, cancellationToken);
    }
  }

  public void Complete() => _events.Writer.TryComplete();
}

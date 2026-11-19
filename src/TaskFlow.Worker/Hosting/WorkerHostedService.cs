using TaskFlow.Worker.Protocol;

namespace TaskFlow.Worker.Hosting;

public sealed partial class WorkerHostedService(
    ILogger<WorkerHostedService> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(
      CancellationToken stoppingToken)
  {
    var worker = new WorkerIdentity(Id: Environment.MachineName);

    try
    {
      LogStartingWorker(worker.Id);
      await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
    finally
    {
      LogWorkerFinished(worker.Id);
    }
  }
}

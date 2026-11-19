using TaskFlow.Worker.Protocol;

namespace TaskFlow.Worker.Hosting;

public sealed partial class WorkerHostedService(
    IConfiguration configuration,
    CoordinatorConnection coordinatorConnection,
    ILogger<WorkerHostedService> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(
      CancellationToken stoppingToken)
  {
    var coordinatorUrl = configuration["TASKFLOW_COORDINATOR_URL"]
                         ?? throw new InvalidOperationException("TASKFLOW_COORDINATOR_URL is not configured.");

    var worker = new WorkerIdentity(
        Id: configuration["TASKFLOW_WORKER_ID"] ?? Environment.MachineName,
        Name: Environment.MachineName);

    LogStartingWorker(worker.Id);

    await coordinatorConnection.RunAsync(
        coordinatorUrl,
        worker,
        stoppingToken);
  }

  [LoggerMessage(1, LogLevel.Information, "Starting worker {WorkerId}.")]
  private partial void LogStartingWorker(string workerId);
}

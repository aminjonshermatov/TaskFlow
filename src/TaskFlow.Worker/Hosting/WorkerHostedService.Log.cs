namespace TaskFlow.Worker.Hosting;

public sealed partial class WorkerHostedService
{
  [LoggerMessage(1, LogLevel.Information, "Starting worker {WorkerId}.")]
  private partial void LogStartingWorker(string workerId);

  [LoggerMessage(2, LogLevel.Information, "Worker {WorkerId} stopped.")]
  private partial void LogWorkerFinished(string workerId);
}

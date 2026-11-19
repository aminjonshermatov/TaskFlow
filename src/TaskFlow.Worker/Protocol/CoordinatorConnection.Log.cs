namespace TaskFlow.Worker.Protocol;

public sealed partial class CoordinatorConnection
{
  [LoggerMessage(1, LogLevel.Information, "Hello sent. Waiting for coordinator.")]
  private partial void LogHelloSent();

  [LoggerMessage(2, LogLevel.Information, "Connected to coordinator {CoordinatorId}.")]
  private partial void LogConnected(string coordinatorId);

  [LoggerMessage(3, LogLevel.Information, "Received assignment for job {JobId}.")]
  private partial void LogJobAssigned(string jobId);

  [LoggerMessage(4, LogLevel.Warning, "Coordinator stream ended.")]
  private partial void LogStreamEnded();

  [LoggerMessage(5, LogLevel.Information, "Worker is stopping.")]
  private partial void LogStopping();

  [LoggerMessage(6, LogLevel.Information, "Worker call was cancelled.")]
  private partial void LogCallCancelled();
}

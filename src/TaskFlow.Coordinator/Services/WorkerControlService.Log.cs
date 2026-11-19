namespace TaskFlow.Coordinator.Services;

public partial class WorkerControlService
{
  [LoggerMessage(1, LogLevel.Information, "Worker stream connected.")]
  private partial void LogStreamConnected();

  [LoggerMessage(2, LogLevel.Information, "Worker registered: {WorkerId} ({WorkerName}), session {SessionId}.")]
  private partial void LogWorkerRegistered(string workerId, string workerName, Guid sessionId);

  [LoggerMessage(3, LogLevel.Debug, "Heartbeat received from worker {WorkerId}.")]
  private partial void LogHeartbeatReceived(string workerId);

  [LoggerMessage(4, LogLevel.Information, "Worker stream was cancelled.")]
  private partial void LogStreamCancelled();

  [LoggerMessage(5, LogLevel.Information, "Worker stream was disconnected.")]
  private partial void LogStreamDisconnected();

  [LoggerMessage(6, LogLevel.Information, "Worker stream was cancelled by client.")]
  private partial void LogStreamCancelledByClient();

  [LoggerMessage(7, LogLevel.Information, "Worker unregistered: {WorkerId}, session {SessionId}, removed: {Removed}.")]
  private partial void LogWorkerUnregistered(string workerId, Guid sessionId, bool removed);

  [LoggerMessage(8, LogLevel.Debug, "Worker response stream closed.")]
  private partial void LogResponseStreamClosed(Exception exception);
}

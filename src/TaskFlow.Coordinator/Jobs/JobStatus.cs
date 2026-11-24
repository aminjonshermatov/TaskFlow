namespace TaskFlow.Coordinator.Jobs;

internal static class JobStatus
{
  public const string Scheduled = "scheduled";
  public const string Queued = "queued";
  public const string Running = "running";
  public const string Succeeded = "succeeded";
  public const string Failed = "failed";
  public const string TimedOut = "timed_out";
  public const string Lost = "lost";
  public const string Retrying = "retrying";
  public const string Canceled = "canceled";
}

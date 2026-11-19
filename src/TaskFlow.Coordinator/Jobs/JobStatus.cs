namespace TaskFlow.Coordinator.Jobs;

public enum JobStatus
{
  Pending,
  Assigned,
  Running,
  Canceled,
  Timeout,
  Success,
  Failed
}

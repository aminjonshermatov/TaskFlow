namespace TaskFlow.Contracts.Jobs;

public enum JobResult
{
  Succeeded = 1,
  Failed = 2,
  TimedOut = 3,
  Canceled = 4,
  Lost = 5
}

namespace TaskFlow.Contracts.Jobs;

[Flags]
public enum RetryOn
{
  None = 0,
  Lost = 1 << 0,
  Failed = 1 << 1,
  TimedOut = 1 << 2
}

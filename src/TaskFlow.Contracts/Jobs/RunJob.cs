namespace TaskFlow.Contracts.Jobs;

public sealed record RunJob(
  Guid JobId, int Attempt, string Command, int TimeoutSeconds);

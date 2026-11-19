namespace TaskFlow.Contracts.Jobs;

public sealed record JobStarted(
  Guid JobId, int Attempt, string WorkerId, DateTimeOffset StartedAt);

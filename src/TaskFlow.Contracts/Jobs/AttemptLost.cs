namespace TaskFlow.Contracts.Jobs;

public sealed record AttemptLost(
  Guid JobId, int Attempt, string ReportedBy);

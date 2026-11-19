namespace TaskFlow.Contracts.Jobs;

public sealed record JobFinished(
  Guid JobId,
  int Attempt,
  string WorkerId,
  JobResult Result,
  int? ExitCode,
  string? Error,
  DateTimeOffset FinishedAt);

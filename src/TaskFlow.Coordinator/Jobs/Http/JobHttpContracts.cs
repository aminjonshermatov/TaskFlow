using TaskFlow.Persistence.Entities;

namespace TaskFlow.Coordinator.Jobs.Http;

public sealed record RetryHttpRequest(
  int? MaxAttempts,
  string[]? RetryOn,
  int? BackoffSeconds);

public sealed record SubmitJobHttpRequest(
  string Command,
  DateTimeOffset StartAt,
  int TimeoutSeconds,
  RetryHttpRequest? Retry);

public sealed record JobHttpResponse(
    Guid Id,
    string Command,
    int TimeoutSeconds,
    string Status,
    string? WorkerId,
    int? ExitCode,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartAt,
    DateTimeOffset? FinishedAt)
{
  public static JobHttpResponse FromDomain(JobState job) =>
      new(
          job.CorrelationId,
          job.Command,
          job.TimeoutSeconds,
          job.CurrentState,
          job.WorkerId,
          job.ExitCode,
          job.CreatedAt,
          job.StartAt,
          job.FinishedAt);
}

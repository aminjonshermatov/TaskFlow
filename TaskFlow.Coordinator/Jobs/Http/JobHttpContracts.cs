namespace TaskFlow.Coordinator.Jobs.Http;

public sealed record SubmitJobHttpRequest(
    string Command,
    int MaxDurationSeconds);

public sealed record JobHttpResponse(
    Guid Id,
    string Command,
    int MaxDurationSeconds,
    string Status,
    string? WorkerId,
    int? ExitCode,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt)
{
    public static JobHttpResponse FromDomain(Job job) =>
        new(
            job.Id,
            job.Command,
            job.MaxDurationSeconds,
            job.Status.ToString().ToLowerInvariant(),
            job.WorkerId,
            job.ExitCode,
            job.CreatedAt,
            job.StartedAt,
            job.FinishedAt);
}
namespace TaskFlow.Coordinator.Jobs;

public sealed record Job(
    Guid Id,
    string Command,
    int MaxDurationSeconds,
    JobStatus Status,
    string? WorkerId,
    int? ExitCode,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt);

using TaskFlow.Contracts.Jobs;

namespace TaskFlow.Coordinator.Jobs;

public sealed record SubmitJobCommand(
    string Command,
    DateTimeOffset StartAt,
    int TimeoutSeconds,
    int MaxAttempts,
    RetryOn RetryOn,
    int BackoffSeconds);

namespace TaskFlow.Coordinator.Jobs;

public sealed record SubmitJobCommand(
    string Command,
    int MaxDurationSeconds);
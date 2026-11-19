namespace TaskFlow.Contracts.Jobs;

public sealed record JobStatusChanged(Guid JobId, string Status);

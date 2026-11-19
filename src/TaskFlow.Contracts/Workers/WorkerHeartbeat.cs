namespace TaskFlow.Contracts.Workers;

public sealed record WorkerHeartbeat(
  string WorkerId,
  string Host,
  Guid? RunningJobId,
  DateTimeOffset At);

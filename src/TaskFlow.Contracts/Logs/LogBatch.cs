namespace TaskFlow.Contracts.Logs;

public sealed record LogBatch(
  Guid JobId,
  int Attempt,
  long FirstSeq,
  IReadOnlyList<LogLine> Lines);

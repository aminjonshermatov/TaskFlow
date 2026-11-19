namespace TaskFlow.Contracts.Logs;

public sealed record LogLine(
  LogStream Stream,
  DateTimeOffset Ts,
  string Text);

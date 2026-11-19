using System.Diagnostics.CodeAnalysis;

namespace TaskFlow.Contracts.Logs;

[SuppressMessage(
  "Naming",
  "CA1711:Identifiers should not have incorrect suffix",
  Justification = "Constants")]
public static class JobLogStream
{
  public const string StreamName = "taskflow.job-logs";

  public static readonly TimeSpan MaxAge = TimeSpan.FromHours(24);

  public const ulong MaxLengthBytes = 1024UL * 1024 * 1024;
}

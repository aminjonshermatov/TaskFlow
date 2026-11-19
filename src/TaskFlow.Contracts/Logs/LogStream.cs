using System.Diagnostics.CodeAnalysis;

namespace TaskFlow.Contracts.Logs;

[SuppressMessage(
  "Naming",
  "CA1711:Identifiers should not have incorrect suffix",
  Justification = "Stderr,Stdout")]
public enum LogStream
{
  Stdout = 1,
  Stderr = 2
}

namespace TaskFlow.Migrator;

internal sealed class CannotCreateStreamException(string message, Exception innerException)
  : Exception(message, innerException);

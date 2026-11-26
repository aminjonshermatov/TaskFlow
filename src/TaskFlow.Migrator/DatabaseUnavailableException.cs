namespace TaskFlow.Migrator;

internal sealed class DatabaseUnavailableException()
  : Exception("Could not connect to database");

using Microsoft.Extensions.Logging;

namespace TaskFlow.Migrator;

internal static partial class MigratorLog
{
  [LoggerMessage(1, LogLevel.Critical, "Migration Failed.")]
  public static partial void MigrationFailed(this ILogger logger, Exception ex);

  [LoggerMessage(2, LogLevel.Information, "Connecting to Database. Attempt: {Retry}/{MaxRetry}.")]
  public static partial void ConnectingToDatabase(this ILogger logger, int retry, int maxRetry);

  [LoggerMessage(3, LogLevel.Warning, "Connection to Database Failed. Attempt: {Retry}/{MaxRetry}, Delay: {Delay}.")]
  public static partial void ConnectionToDatabaseFailed(this ILogger logger, int retry, int maxRetry, TimeSpan delay);

  [LoggerMessage(4, LogLevel.Information, "Migration finished successfully.")]
  public static partial void MigratedSuccessfully(this ILogger logger);
}

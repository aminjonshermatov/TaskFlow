using Microsoft.Extensions.Logging;

namespace TaskFlow.Migrator;

internal static partial class MigratorLog
{
  [LoggerMessage(1, LogLevel.Information, "Connecting to {Resource}. Attempt: {Retry}/{MaxRetry}.")]
  public static partial void ConnectingToResource(this ILogger logger, string resource, int retry, int maxRetry);

  [LoggerMessage(2, LogLevel.Warning, "Connection to {Resource} Failed. Attempt: {Retry}/{MaxRetry}, Delay: {Delay}.")]
  public static partial void ConnectionToResourceFailed(this ILogger logger, string resource, int retry, int maxRetry, TimeSpan delay);

  [LoggerMessage(3, LogLevel.Information, "Migration finished successfully.")]
  public static partial void MigratedSuccessfully(this ILogger logger);

  [LoggerMessage(4, LogLevel.Critical, "Could not create stream.")]
  public static partial void CannotCreateStream(this ILogger logger, Exception ex);

  [LoggerMessage(5, LogLevel.Critical, "Migrator did not finish within the total timeout of {TimeoutSeconds}s.")]
  public static partial void TotalTimeoutExceeded(this ILogger logger, Exception ex, int timeoutSeconds);

  [LoggerMessage(6, LogLevel.Critical, "Validations failed.")]
  public static partial void OptionsValidationFailed(this ILogger logger, Exception ex);

  [LoggerMessage(7, LogLevel.Critical, "Unexpected Error.")]
  public static partial void UnexpectedError(this ILogger logger, Exception ex);

  [LoggerMessage(8, LogLevel.Critical, "Dependency did not respond after {MaxRetry} attempts.")]
  public static partial void RetriesExhausted(this ILogger logger, Exception ex, int maxRetry);
}

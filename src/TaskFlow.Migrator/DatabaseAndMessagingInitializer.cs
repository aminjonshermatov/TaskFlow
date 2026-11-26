using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

using TaskFlow.Contracts.Logs;
using TaskFlow.Persistence;
using TaskFlow.ServiceDefaults.Options;

namespace TaskFlow.Migrator;

internal static class DatabaseAndMessagingInitializer
{
  public static async Task MakeDatabaseMigrationsAsync(
  TaskFlowDbContext dbContext,
  BackoffOptions backoffOptions,
  TimeProvider timeProvider,
  ILogger logger,
  CancellationToken ct)
  {
    await RetryAsync(
      async cancellationToken =>
      {
        if (!await dbContext.Database.CanConnectAsync(cancellationToken))
        {
          throw new DatabaseUnavailableException();
        }
        return true;
      },
      ex => ex is DatabaseUnavailableException,
      "DbConnection",
      backoffOptions,
      logger,
      timeProvider,
      ct
    );
    await dbContext.Database.MigrateAsync(ct);
    logger.MigratedSuccessfully();
  }

  public static async Task ConfigureRabbitMqStreamAsync(
    RabbitMqOptions rabbitMqOptions,
    BackoffOptions backoffOptions,
    TimeProvider timeProvider,
    ILogger logger,
    CancellationToken ct)
  {
    var rabbitMqFactory = new ConnectionFactory
    {
      HostName = rabbitMqOptions.Host,
      VirtualHost = rabbitMqOptions.VHost,
      Port = rabbitMqOptions.Port,
      UserName = rabbitMqOptions.Username,
      Password = rabbitMqOptions.Password
    };

    await using var connection = await RetryAsync(
      rabbitMqFactory.CreateConnectionAsync,
      ex => ex is BrokerUnreachableException,
      "RabbitMQConnection",
      backoffOptions,
      logger,
      timeProvider,
      ct
    );

    try
    {
      await using var channel = await connection.CreateChannelAsync(null, ct);

      var arguments = new Dictionary<string, object?>
      {
        { "x-queue-type", "stream" },
        { "x-max-length-bytes", JobLogStream.MaxLengthBytes },
        { "x-max-age", JobLogStream.MaxAge.ToRabbitMqDuration() }
      };

      await channel.QueueDeclareAsync(
        queue: JobLogStream.StreamName,
        durable: true,
        exclusive: false,
        autoDelete: false,
        arguments: arguments,
        cancellationToken: ct
      );
    }
    catch (OperationInterruptedException operationInterruptedException)
      when (operationInterruptedException.ShutdownReason is { ReplyCode: 406 } reason)
    {
      throw new CannotCreateStreamException(
        $"Stream '{JobLogStream.StreamName}' already exists with different arguments. Change retention via a RabbitMQ policy or delete the stream. Broker: {reason.ReplyText}.",
        operationInterruptedException);
    }
  }

  private static async Task<T> RetryAsync<T>(
    Func<CancellationToken, Task<T>> operation,
    Func<Exception, bool> isTransient,
    string resourceName,
    BackoffOptions backoffOptions,
    ILogger logger,
    TimeProvider timeProvider,
    CancellationToken ct)
  {
    Exception? innerException = null;
    TimeSpan delay = TimeSpan.FromSeconds(backoffOptions.InitialDelayInSeconds);
    for (int retry = 1; retry <= backoffOptions.MaxRetry; ++retry, delay = delay.Multiply(backoffOptions.DelayMultiplier))
    {
      logger.ConnectingToResource(resourceName, retry, backoffOptions.MaxRetry);
      try
      {
        return await operation(ct);
      }
      catch (Exception ex) when (isTransient(ex))
      {
        innerException = ex;
      }

      if (retry < backoffOptions.MaxRetry)
      {
        logger.ConnectionToResourceFailed(resourceName, retry, backoffOptions.MaxRetry, delay);
        await Task.Delay(delay, timeProvider, ct);
      }
    }
    throw new TimeoutException($"Could not connect to {resourceName}", innerException);
  }
}

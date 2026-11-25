using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TaskFlow.Migrator;
using TaskFlow.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.AddPostgres();
builder.Services
  .AddOptions<BackoffOptions>()
  .Bind(builder.Configuration.GetSection(BackoffOptions.SectionName));
builder.Services.AddSingleton<IValidateOptions<BackoffOptions>, ValidateBackoffOptions>();

using var app = builder.Build();

var timeProvider = TimeProvider.System;

var log = app.Services.GetRequiredService<ILogger<Program>>();
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
try
{
  var backoffOptions = app.Services.GetRequiredService<IOptions<BackoffOptions>>().Value;

  await using var scope = app.Services.CreateAsyncScope();
  var db = scope.ServiceProvider.GetRequiredService<TaskFlowDbContext>();
  await WaitForDatabaseAsync(
    db, log,
    backoffOptions,
    timeProvider,
    cts.Token);
  await db.Database.MigrateAsync(cts.Token);
  log.MigratedSuccessfully();
  return 0;
}
catch (Exception ex)
{
  log.MigrationFailed(ex);
  return 1;
}

static async Task WaitForDatabaseAsync(
  TaskFlowDbContext dbContext,
  ILogger logger,
  BackoffOptions backoffOptions,
  TimeProvider timeProvider,
  CancellationToken cancellationToken)
{
  TimeSpan delay = TimeSpan.FromSeconds(backoffOptions.InitialDelayInSeconds);
  for (int retry = 1; retry <= backoffOptions.MaxRetry; ++retry, delay = delay.Multiply(backoffOptions.DelayMultiplier))
  {
    logger.ConnectingToDatabase(retry, backoffOptions.MaxRetry);
    if (await dbContext.Database.CanConnectAsync(cancellationToken))
    {
      return;
    }
    logger.ConnectionToDatabaseFailed(retry, backoffOptions.MaxRetry, delay);
    if (retry < backoffOptions.MaxRetry)
    {
      await Task.Delay(delay, timeProvider, cancellationToken);
    }
  }
  throw new TimeoutException("Could not connect to database");
}

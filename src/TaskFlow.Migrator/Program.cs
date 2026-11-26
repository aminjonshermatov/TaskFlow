using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TaskFlow.Migrator;
using TaskFlow.Persistence;
using TaskFlow.Persistence.Options;
using TaskFlow.ServiceDefaults;
using TaskFlow.ServiceDefaults.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.AddPostgres();
builder.AddServiceDefaults();

builder.Services
  .AddOptions<BackoffOptions>()
  .Bind(builder.Configuration.GetSection(BackoffOptions.SectionName));
builder.Services.AddSingleton<IValidateOptions<BackoffOptions>, ValidateBackoffOptions>();

using var app = builder.Build();

var log = app.Services.GetRequiredService<ILogger<Program>>();

BackoffOptions backoffOptions;
RabbitMqOptions rabbitMqOptions;
try
{
  backoffOptions = app.Services.GetRequiredService<IOptions<BackoffOptions>>().Value;
  rabbitMqOptions = app.Services.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
  _ = app.Services.GetRequiredService<IOptions<PostgresOptions>>().Value;
}
catch (OptionsValidationException optionsValidationException)
{
  log.OptionsValidationFailed(optionsValidationException);
  return 1;
}

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(backoffOptions.TotalTimeoutSeconds));

try
{
  var timeProvider = TimeProvider.System;
  await using var scope = app.Services.CreateAsyncScope();

  var db = scope.ServiceProvider.GetRequiredService<TaskFlowDbContext>();

  await DatabaseAndMessagingInitializer.MakeDatabaseMigrationsAsync(db, backoffOptions, timeProvider, log, cts.Token);
  await DatabaseAndMessagingInitializer.ConfigureRabbitMqStreamAsync(rabbitMqOptions, backoffOptions, timeProvider, log,
    cts.Token);
  return 0;
}
catch (OperationCanceledException operationCanceledException) when (cts.IsCancellationRequested)
{
  log.TotalTimeoutExceeded(operationCanceledException, backoffOptions.TotalTimeoutSeconds);
  return 1;
}
catch (CannotCreateStreamException streamException)
{
  log.CannotCreateStream(streamException);
  return 1;
}
catch (TimeoutException timeoutException)
{
  log.RetriesExhausted(timeoutException, backoffOptions.MaxRetry);
  return 1;
}
catch (Exception ex)
{
  log.UnexpectedError(ex);
  return 1;
}

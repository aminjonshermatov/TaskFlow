using MassTransit;
using MassTransit.Logging;
using MassTransit.Monitoring;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using TaskFlow.ServiceDefaults.Options;

namespace TaskFlow.ServiceDefaults;

public static class ServiceDefaultsExtensions
{
  private const string HealthEndpointPath = "/healthz";

  public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
  {
    builder.Services
      .AddOptions<RabbitMqOptions>()
      .BindConfiguration(RabbitMqOptions.SectionName)
      .ValidateOnStart();
    builder.Services.AddSingleton<IValidateOptions<RabbitMqOptions>, ValidateRabbitMqOptions>();

    builder.Logging.AddOpenTelemetry(logging =>
    {
      logging.IncludeFormattedMessage = true;
      logging.IncludeScopes = true;
    });

    builder.AddOpenTelemetry()
      .ConfigureResource(configuration =>
      {
        configuration.AddService(serviceName: builder.Environment.ApplicationName);
      })
      .WithMetrics(metrics =>
      {
        metrics
          .AddMeter(OtelConstants.TaskFlow)
          .AddMeter(OtelConstants.SystemRuntime)
          .AddMeter(InstrumentationOptions.MeterName)
          .AddAspNetCoreInstrumentation()
          .AddNpgsqlInstrumentation()
          .AddHttpClientInstrumentation();
      })
      .WithTracing(tracing =>
      {
        tracing
          .AddSource(OtelConstants.TaskFlow)
          .AddSource(DiagnosticHeaders.DefaultListenerName)
          .AddAspNetCoreInstrumentation(instrumentationOptions =>
          {
            instrumentationOptions.Filter =
              ctx => !ctx.Request.Path.StartsWithSegments(HealthEndpointPath);
          })
          .AddNpgsql()
          .AddHttpClientInstrumentation();
      });

    builder.AddOpenTelemetryExporters();

    builder.Services.AddHealthChecks()
      .AddCheck("self", () => HealthCheckResult.Healthy(), tags: [HealthTags.Live]);

    return builder;
  }

  public static WebApplication MapDefaultEndpoints(this WebApplication app)
  {
    app
      .MapHealthChecks($"{HealthEndpointPath}/{HealthTags.Live}", new HealthCheckOptions
      {
        Predicate = registration => registration.Tags.Contains(HealthTags.Live),
      })
      .DisableHttpMetrics();
    app
      .MapHealthChecks($"{HealthEndpointPath}/{HealthTags.Ready}", new HealthCheckOptions
      {
        Predicate = registration => registration.Tags.Contains(HealthTags.Ready),
        ResultStatusCodes =
        {
          [HealthStatus.Healthy] = StatusCodes.Status200OK,
          [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
          [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        }
      })
      .DisableHttpMetrics();
    return app;
  }

  extension(IBusRegistrationConfigurator configurator)
  {
    public void ApplyHealthCheckOptions()
    {
      configurator.ConfigureHealthCheckOptions(options =>
      {
        options.Name = "bus";
        options.Tags.Clear();
        options.Tags.Add(HealthTags.Ready);
        options.MinimalFailureStatus = HealthStatus.Unhealthy;
      });
    }
  }

  public static TBuilder AddMessaging<TBuilder>(this TBuilder builder, Action<IBusRegistrationConfigurator>? registerConsumers = null) where TBuilder : IHostApplicationBuilder
  {
    builder.Services.AddMassTransit(configure =>
    {
      registerConsumers?.Invoke(configure);
      configure.ApplyHealthCheckOptions();
      configure.SetKebabCaseEndpointNameFormatter();
      configure.UsingRabbitMq((ctx, cfg) =>
      {
        var rabbitMqOptions = ctx.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
        var env = ctx.GetRequiredService<IHostEnvironment>();
        cfg.Host(
          rabbitMqOptions.Host,
          rabbitMqOptions.Port,
          rabbitMqOptions.VHost,
          $"{env.ApplicationName}@{Environment.MachineName}",
          configurator =>
          {
            configurator.Username(rabbitMqOptions.Username);
            configurator.Password(rabbitMqOptions.Password);
            configurator.Heartbeat(TimeSpan.FromSeconds(rabbitMqOptions.HeartBeatSeconds));
          });
        cfg.ConfigureEndpoints(ctx);
      });
    });

    return builder;
  }

  private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder)
    where TBuilder : IHostApplicationBuilder
  {
    var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
    if (!useOtlpExporter) return builder;

    builder.Services.ConfigureOpenTelemetryLoggerProvider(loggerProvider =>
      loggerProvider.AddOtlpExporter());
    builder.Services.ConfigureOpenTelemetryMeterProvider(meterProvider =>
      meterProvider.AddOtlpExporter());
    builder.Services.ConfigureOpenTelemetryTracerProvider(tracerProvider =>
      tracerProvider.AddOtlpExporter());
    return builder;
  }
}


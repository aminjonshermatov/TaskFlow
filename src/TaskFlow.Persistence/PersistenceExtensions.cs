using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using TaskFlow.Persistence.Options;

namespace TaskFlow.Persistence;

public static class PersistenceExtensions
{
  public static TBuilder AddPostgres<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
  {
    builder.Services
      .AddOptions<PostgresOptions>()
      .BindConfiguration(PostgresOptions.SectionName)
      .ValidateOnStart();
    builder.Services.AddSingleton<IValidateOptions<PostgresOptions>, ValidatePostgresOptions>();

    builder.Services.AddDbContextPool<TaskFlowDbContext>((serviceProvider, optionsBuilder) =>
    {
      var pgOptions = serviceProvider.GetRequiredService<IOptions<PostgresOptions>>().Value;
      TaskFlowDbContext.ConfigureOptions(optionsBuilder, pgOptions.ConnectionString);
    });

    return builder;
  }
}

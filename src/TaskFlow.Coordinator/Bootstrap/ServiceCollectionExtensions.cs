using Npgsql;

using TaskFlow.Coordinator.Jobs;

namespace TaskFlow.Coordinator.Bootstrap;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddTaskFlowCoordinator(this IServiceCollection services, IConfiguration configuration)
  {
    var postgresConnectionString = configuration.GetConnectionString("Postgres")
                                   ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

    services.AddSingleton<NpgsqlDataSource>(_ =>
    {
      var builder = new NpgsqlDataSourceBuilder(postgresConnectionString);
      builder.MapEnum<JobStatus>("job_status");

      return builder.Build();
    });

    services.AddSingleton(TimeProvider.System);

    services.AddSingleton<IJobRepository, DapperJobRepository>();
    services.AddSingleton<JobService>();

    return services;
  }
}

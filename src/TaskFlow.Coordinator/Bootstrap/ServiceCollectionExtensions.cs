using Npgsql;

using TaskFlow.Coordinator.Jobs;
using TaskFlow.Coordinator.Workers;

namespace TaskFlow.Coordinator.Bootstrap;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddTaskFlowCoordinator(this IServiceCollection services, IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("TaskFlow")
                           ?? throw new InvalidOperationException("Connection string 'TaskFlow' is not configured.");

    services.AddSingleton<NpgsqlDataSource>(_ =>
    {
      var builder = new NpgsqlDataSourceBuilder(connectionString);
      builder.MapEnum<JobStatus>("job_status");

      return builder.Build();
    });

    services.AddSingleton(TimeProvider.System);

    services.AddSingleton<IWorkerRegistry, WorkerRegistry>();
    services.AddSingleton<IJobRepository, DapperJobRepository>();
    services.AddSingleton<JobService>();

    return services;
  }
}

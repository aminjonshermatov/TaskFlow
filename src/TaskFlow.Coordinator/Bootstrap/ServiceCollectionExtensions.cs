using TaskFlow.Coordinator.Jobs;
using TaskFlow.Persistence;

namespace TaskFlow.Coordinator.Bootstrap;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddTaskFlowCoordinator(this IServiceCollection services, IConfiguration configuration)
  {
    var postgresConnectionString = configuration.GetConnectionString("Postgres")
                                   ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");
    services.AddDbContextPool<TaskFlowDbContext>(optionsBuilder => TaskFlowDbContext.ConfigureOptions(optionsBuilder, postgresConnectionString));

    services.AddSingleton(TimeProvider.System);
    services.AddScoped<JobService>();

    return services;
  }
}

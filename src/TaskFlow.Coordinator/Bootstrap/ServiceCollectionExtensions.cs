using TaskFlow.Persistence;
using TaskFlow.ServiceDefaults;

namespace TaskFlow.Coordinator.Bootstrap;

public static class ServiceCollectionExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddDbContextHealthCheck()
    {
      services.AddHealthChecks().AddDbContextCheck<TaskFlowDbContext>("postgres", tags: [HealthTags.Ready]);

      return services;
    }
  }
}

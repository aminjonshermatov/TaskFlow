namespace TaskFlow.Coordinator.Jobs;

public interface IJobRepository
{
  Task CreateAsync(Job job, CancellationToken cancellationToken);
  Task<Job?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken);
}

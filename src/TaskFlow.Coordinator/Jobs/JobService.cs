namespace TaskFlow.Coordinator.Jobs;

public sealed class JobService(IJobRepository jobRepository, TimeProvider timeProvider)
{
  public async Task<Job> SubmitAsync(SubmitJobCommand command, CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(command);
    ArgumentException.ThrowIfNullOrWhiteSpace(command.Command);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(command.MaxDurationSeconds);

    var now = timeProvider.GetUtcNow();
    var job = new Job(
        Id: Guid.CreateVersion7(now),
        Command: command.Command,
        MaxDurationSeconds: command.MaxDurationSeconds,
        Status: JobStatus.Pending,
        WorkerId: null,
        ExitCode: null,
        CreatedAt: now,
        StartedAt: null,
        FinishedAt: null);

    await jobRepository.CreateAsync(job, cancellationToken);
    return job;
  }
}

namespace TaskFlow.Coordinator.Jobs;

public sealed class JobService(IJobRepository jobRepository, TimeProvider timeProvider)
{
    public async Task<Job> SubmitAsync(SubmitJobCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Command))
        {
            throw new ArgumentException("Command must not be empty.", nameof(command.Command));
        }

        if (command.MaxDurationSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(command.MaxDurationSeconds), "Maximum duration must be positive.");
        }

        var job = new Job(
            Id: Guid.NewGuid(),
            Command: command.Command,
            MaxDurationSeconds: command.MaxDurationSeconds,
            Status: JobStatus.Pending,
            WorkerId: null,
            ExitCode: null,
            CreatedAt: timeProvider.GetUtcNow(),
            StartedAt: null,
            FinishedAt: null);

        await jobRepository.CreateAsync(job, cancellationToken);
        return job;
    }
}
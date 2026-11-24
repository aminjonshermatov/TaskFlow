using TaskFlow.Persistence;
using TaskFlow.Persistence.Entities;

namespace TaskFlow.Coordinator.Jobs;

public sealed class JobService(TaskFlowDbContext dbContext, TimeProvider timeProvider)
{
  public async Task<JobState> SubmitAsync(SubmitJobCommand command, CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(command);
    ArgumentException.ThrowIfNullOrWhiteSpace(command.Command);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(command.TimeoutSeconds);

    var now = timeProvider.GetUtcNow();
    var startAt = command.StartAt.ToUniversalTime();
    var jobState = new JobState
    {
      CorrelationId = Guid.CreateVersion7(now),
      CurrentState = JobStatus.Scheduled,
      Command = command.Command,
      TimeoutSeconds = command.TimeoutSeconds,
      BackoffSeconds = command.BackoffSeconds,
      MaxAttempts = command.MaxAttempts,
      RetryOn = command.RetryOn,
      DueAt = startAt,
      StartAt = startAt,
      CreatedAt = now,
      UpdatedAt = now,
    };

    await dbContext.Jobs.AddAsync(jobState, cancellationToken);
    await dbContext.SaveChangesAsync(cancellationToken);
    return jobState;
  }
}

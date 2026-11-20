using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TaskFlow.Contracts.Jobs;

namespace TaskFlow.Persistence.Entities;

internal sealed class JobStateConfiguration : IEntityTypeConfiguration<JobState>
{
  public void Configure(EntityTypeBuilder<JobState> builder)
  {
    builder.ToTable("jobs");

    builder.HasKey(jobState => jobState.CorrelationId);
    builder.HasIndex(jobState => jobState.DueAt).HasFilter("due_at is not null");
    builder.HasIndex(jobState => jobState.IdempotencyKey).IsUnique().HasFilter("idempotency_key is not null");
    builder.HasIndex(jobState => new { jobState.CurrentState, jobState.CorrelationId }).IsDescending(false, true);
  }
}

public sealed class JobState : SagaStateMachineInstance
{
  public Guid CorrelationId { get; set; }
  public string CurrentState { get; set; } = null!;

  public string Command { get; set; } = null!;
  public DateTimeOffset StartAt { get; set; }
  public int TimeoutSeconds { get; set; }
  public int MaxAttempts { get; set; } = 1;
  public RetryOn RetryOn { get; set; } = RetryOn.Lost;
  public int BackoffSeconds { get; set; }

  public int CurrentAttempt { get; set; }
  public DateTimeOffset? DueAt { get; set; }
  public string? WorkerId { get; set; }
  public int? ExitCode { get; set; }
  public string? Error { get; set; }
  public DateTimeOffset? CancelRequestedAt { get; set; }

  public string? IdempotencyKey { get; set; }
  public byte[]? RequestHash { get; set; }

  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset? FinishedAt { get; set; }
  public DateTimeOffset UpdatedAt { get; set; }
}

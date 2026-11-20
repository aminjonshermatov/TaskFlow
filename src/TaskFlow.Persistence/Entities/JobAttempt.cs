using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TaskFlow.Contracts.Jobs;

namespace TaskFlow.Persistence.Entities;

internal sealed class JobAttemptConfiguration : IEntityTypeConfiguration<JobAttempt>
{
  public void Configure(EntityTypeBuilder<JobAttempt> builder)
  {
    builder.ToTable("job_attempts");

    builder.HasKey(jobAttempt => new { jobAttempt.JobId, jobAttempt.Attempt });
    builder.HasOne<JobState>().WithMany().HasForeignKey(job => job.JobId);
  }
}

public sealed class JobAttempt
{
  public Guid JobId { get; set; }
  public int Attempt { get; set; }
  public string WorkerId { get; set; } = null!;

  public JobResult? Result { get; set; }
  public int? ExitCode { get; set; }

  public DateTimeOffset StartedAt { get; set; }
  public DateTimeOffset? FinishedAt { get; set; }
}

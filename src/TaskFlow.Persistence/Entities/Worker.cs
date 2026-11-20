using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaskFlow.Persistence.Entities;

internal sealed class WorkerConfiguration : IEntityTypeConfiguration<Worker>
{
  public void Configure(EntityTypeBuilder<Worker> builder)
  {
    builder.ToTable("workers");

    builder.HasKey(x => x.Id);
  }
}

public sealed class Worker
{
  public string Id { get; set; } = null!;
  public string Host { get; set; } = null!;
  public Guid? RunningJobId { get; set; }
  public DateTimeOffset LastSeenAt { get; set; }
}

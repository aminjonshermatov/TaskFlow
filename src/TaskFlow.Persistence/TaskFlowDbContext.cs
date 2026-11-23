using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskFlow.Persistence;

public sealed class TaskFlowDbContext(DbContextOptions<TaskFlowDbContext> options)
  : DbContext(options)
{
  public static void ConfigureOptions(DbContextOptionsBuilder builder, string connectionString)
  {
    builder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
  }
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskFlowDbContext).Assembly);
    modelBuilder.AddInboxStateEntity();
    modelBuilder.AddOutboxStateEntity();
    modelBuilder.AddOutboxMessageEntity();
  }
}

public sealed class TaskFlowDbContextFactory : IDesignTimeDbContextFactory<TaskFlowDbContext>
{
  public TaskFlowDbContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<TaskFlowDbContext>();
    TaskFlowDbContext.ConfigureOptions(optionsBuilder, "");
    return new(optionsBuilder.Options);
  }
}

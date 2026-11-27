using System.Text.Json.Nodes;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using TaskFlow.Contracts.Logs;
using TaskFlow.IntegrationTests.Fixtures;
using TaskFlow.Migrator;
using TaskFlow.Persistence;

namespace TaskFlow.IntegrationTests.Migrator;

[Collection(InfraCollectionDefinition.Name)]
public sealed class MigratorTests(InfraFixture infraFixture)
{
  private static CancellationToken Ct => TestContext.Current.CancellationToken;

  [Fact]
  public async Task Initialize_OnEmptyDatabase_AppliesMigrationAndCreatesStream()
  {
    await using var dbContext = MakeDbContext(await infraFixture.CreateDatabaseAsync(Ct));

    await RunMigrationAsync(dbContext, Ct);
    await AssertMigrationAsync(dbContext, Ct);
  }

  [Fact]
  public async Task Initialize_RunTwice_IsIdempotent()
  {
    var connectionString = await infraFixture.CreateDatabaseAsync(Ct);

    for (int i = 0; i < 2; ++i)
    {
      await using var dbContext = MakeDbContext(connectionString);
      await RunMigrationAsync(dbContext, Ct);
      await AssertMigrationAsync(dbContext, Ct);
    }
  }

  private static TaskFlowDbContext MakeDbContext(string connectionString)
  {
    var optionsBuilder = new DbContextOptionsBuilder<TaskFlowDbContext>();
    TaskFlowDbContext.ConfigureOptions(optionsBuilder, connectionString);
    return new TaskFlowDbContext(optionsBuilder.Options);
  }

  private async Task RunMigrationAsync(TaskFlowDbContext dbContext, CancellationToken ct)
  {
    var backoffOptions = new BackoffOptions
    {
      MaxRetry = 2,
      InitialDelayInSeconds = 1
    };

    await DatabaseAndMessagingInitializer.MakeDatabaseMigrationsAsync(
      dbContext,
      backoffOptions,
      TimeProvider.System,
      NullLogger.Instance,
      ct);
    await DatabaseAndMessagingInitializer.ConfigureRabbitMqStreamAsync(
      infraFixture.CreateRabbitMqOptions(),
      backoffOptions,
      TimeProvider.System,
      NullLogger.Instance,
      ct);
  }

  private async Task AssertMigrationAsync(TaskFlowDbContext dbContext, CancellationToken ct)
  {
    Assert.Empty(await dbContext.Database.GetPendingMigrationsAsync(ct));

    var tablesQuery = dbContext.Database.SqlQuery<string>($"""select table_name as "Value" from information_schema.tables where table_schema = 'public'""");
    HashSet<string> expected = ["inbox_state", "outbox_state", "outbox_message"];
    Assert.Superset(expected, await tablesQuery.ToHashSetAsync(ct));

    var execResult =
      await infraFixture.RabbitMq.ExecAsync(["rabbitmqctl", "list_queues", "name", "type", "--formatter", "json"], ct);
    Assert.True(execResult.ExitCode == 0, execResult.Stderr);
    var queues = JsonNode.Parse(execResult.Stdout)?.AsArray() ?? [];
    Assert.Contains(queues,
      queue => queue?["name"]?.GetValue<string>() == JobLogStream.StreamName && queue?["type"]?.GetValue<string>() == "stream");
  }
}

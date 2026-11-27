using Npgsql;

using TaskFlow.ServiceDefaults.Options;

using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace TaskFlow.IntegrationTests.Fixtures;

public sealed class InfraFixture : IAsyncLifetime
{
  public PostgreSqlContainer Postgres { get; } = new PostgreSqlBuilder(Constants.Postgres.DockerImage)
    .WithUsername(Constants.Postgres.Username)
    .WithPassword(Constants.Postgres.Password)
    .Build();

  public RabbitMqContainer RabbitMq { get; } = new RabbitMqBuilder(Constants.RabbitMq.DockerImage)
    .WithUsername(Constants.RabbitMq.Username)
    .WithPassword(Constants.RabbitMq.Password)
    .WithResourceMapping(
      new FileInfo(Path.Combine(AppContext.BaseDirectory, "rabbitmq", "enabled_plugins")),
      new FileInfo("/etc/rabbitmq/enabled_plugins"))
    .Build();

  public RabbitMqOptions CreateRabbitMqOptions()
  {
    return new RabbitMqOptions
    {
      Host = RabbitMq.Hostname,
      Port = RabbitMq.GetMappedPublicPort(5672),
      Username = Constants.RabbitMq.Username,
      Password = Constants.RabbitMq.Password,
    };
  }

  public async ValueTask InitializeAsync()
  {
    await Task.WhenAll(
      Postgres.StartAsync(),
      RabbitMq.StartAsync()
    );
  }

  public async ValueTask DisposeAsync()
  {
    await Task.WhenAll(
      Postgres.DisposeAsync().AsTask(),
      RabbitMq.DisposeAsync().AsTask()
    );
  }

  public async Task<string> CreateDatabaseAsync(CancellationToken ct)
  {
    var connectionString = Postgres.GetConnectionString();
    var dbName = $"test_{Guid.CreateVersion7():N}";

    await using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync(ct);

    await using var command = new NpgsqlCommand($"""create database "{dbName}" """, connection);
    await command.ExecuteNonQueryAsync(ct);

    return new NpgsqlConnectionStringBuilder(connectionString) { Database = dbName }.ConnectionString;
  }
}

[CollectionDefinition(Name)]
public class InfraCollectionDefinition : ICollectionFixture<InfraFixture>
{
  public const string Name = "Infra";
}

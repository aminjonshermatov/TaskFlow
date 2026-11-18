using DbUp;

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__TaskFlow")
                       ?? throw new InvalidOperationException(
                           "Environment variable ConnectionStrings__TaskFlow is required.");

var scriptsDirectory = Path.Combine(AppContext.BaseDirectory, "Scripts");

if (!Directory.Exists(scriptsDirectory))
{
    throw new DirectoryNotFoundException(
        $"Migrations directory was not found: {scriptsDirectory}");
}

var upgrader = DeployChanges.To
    .PostgresqlDatabase(connectionString)
    .WithScriptsFromFileSystem(scriptsDirectory)
    .JournalToPostgresqlTable("public", "taskflow_schema_versions")
    .WithTransaction()
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    Console.Error.WriteLine(result.Error);
    return 1;
}

Console.WriteLine("Database is up to date.");
return 0;
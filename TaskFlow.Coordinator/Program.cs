using Npgsql;
using TaskFlow.Coordinator.Services;
using TaskFlow.Coordinator.Workers;
using TaskFlow.Coordinator.Jobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddSingleton<IWorkerRegistry,  WorkerRegistry>();

var connectionString = builder.Configuration.GetConnectionString("TaskFlow")
                       ?? throw new InvalidOperationException("Connection string 'TaskFlow' is not configured.");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<JobStatus>("job_status");

builder.Services.AddSingleton(dataSourceBuilder.Build());
builder.Services.AddSingleton<IJobRepository, DapperJobRepository>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<JobService>();

var app = builder.Build();

if (!app.Environment.IsEnvironment("Container"))
{
    app.UseHttpsRedirection();
}

app.MapGrpcService<WorkerControlService>();
app.MapGrpcService<CoordinatorQueryService>();

app.MapGet("/", () => "TaskFlow Coordinator is running.");
app.MapGet("/api/workers", (IWorkerRegistry registry) => registry.GetAll());

app.Run();

using TaskFlow.Coordinator.Services;
using TaskFlow.Coordinator.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddSingleton<IWorkerRegistry,  WorkerRegistry>();

var app = builder.Build();

app.MapGrpcService<WorkerControlService>();
app.MapGrpcService<CoordinatorQueryService>();

app.MapGet("/", () => "TaskFlow Coordinator is running.");
app.MapGet("/api/workers", (IWorkerRegistry registry) => registry.GetAll());

app.Run();

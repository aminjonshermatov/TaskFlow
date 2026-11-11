using TaskScheduler.Coordinator.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<WorkerControlService>();
app.MapGet("/", () => "TaskFlow Coordinator is running.");

app.Run();

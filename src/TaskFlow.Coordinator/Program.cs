using TaskFlow.Coordinator.Bootstrap;
using TaskFlow.Coordinator.Jobs.Http;
using TaskFlow.Coordinator.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddTaskFlowCoordinator(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsEnvironment("Container"))
{
  app.UseHttpsRedirection();
}

app.MapGet("/ping", () => "pong");

app.MapGrpcService<WorkerControlService>();
app.MapJobEndpoints();

app.Run();

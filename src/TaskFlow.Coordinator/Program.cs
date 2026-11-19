using TaskFlow.Coordinator.Bootstrap;
using TaskFlow.Coordinator.Jobs.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTaskFlowCoordinator(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsEnvironment("Container"))
{
  app.UseHttpsRedirection();
}

app.MapGet("/ping", () => "pong");

app.MapJobEndpoints();

app.Run();

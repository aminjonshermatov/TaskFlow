using TaskFlow.Coordinator.Bootstrap;
using TaskFlow.Coordinator.Jobs;
using TaskFlow.Coordinator.Jobs.Http;
using TaskFlow.Persistence;
using TaskFlow.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextHealthCheck();
builder.AddServiceDefaults();
builder.AddMessaging();
builder.AddPostgres();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<JobService>();

var app = builder.Build();

app.MapGet("/ping", () => "pong");

app.MapDefaultEndpoints();
app.MapJobEndpoints();

app.Run();

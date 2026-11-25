using TaskFlow.ServiceDefaults;
using TaskFlow.Worker.Hosting;

var builder = WebApplication.CreateSlimBuilder(args);
builder.AddServiceDefaults();
builder.AddMessaging();

builder.Services.AddHostedService<WorkerHostedService>();

var app = builder.Build();
app.MapDefaultEndpoints();

await app.RunAsync();

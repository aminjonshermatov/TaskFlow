using TaskFlow.Worker.Hosting;
using TaskFlow.Worker.Protocol;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<CoordinatorConnection>();
builder.Services.AddHostedService<WorkerHostedService>();

await builder.Build().RunAsync();
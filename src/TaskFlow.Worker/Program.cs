using TaskFlow.Worker.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<WorkerHostedService>();

await builder.Build().RunAsync();

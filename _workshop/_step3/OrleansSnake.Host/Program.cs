using OrleansSnake.Host.Helpers;
using OrleansSnake.Host.Hubs;
using OrleansSnake.Host.Workers;
using Orleans.Configuration;
using System.Net;
using OrleansSnake.Host.Grains;


// Web Application Builder
var builder = WebApplication.CreateBuilder(args);


// Configuration
builder.Configuration.AddEnvironmentVariables();


// Dependency Injection
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
builder.Services.AddSingleton<TickerHub>();
builder.Services.AddSingleton<GameHelper>();
builder.Services.AddHostedService<TickerWorker>();


// Orleans Hosting
builder.Host.UseOrleans((hostBuilder, siloBuilder) =>
{
    siloBuilder.UseLocalhostClustering(siloPort: 11112, gatewayPort: 30001, primarySiloEndpoint: new IPEndPoint(IPAddress.Loopback, 11112), serviceId: "orleans-snake-host", clusterId: "orleans-snake-host");

    siloBuilder.Configure<ClusterOptions>(options =>
    {
        options.ClusterId = "orleans-snake-host";
        options.ServiceId = "orleans-snake-host";
    });

    siloBuilder.ConfigureLogging(loggingBuilder =>
    {
        loggingBuilder.AddConsole();
    });

    siloBuilder.UseDashboard(opt =>
    {
        opt.HostSelf = true;
    });
});

var app = builder.Build();


// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



// Map Orleans dashboard endpoint
app.Map("/dashboard", x => x.UseOrleansDashboard());


// Map HTTP endpoints using minimal API's
app.MapGet("/status",
    (IGrainFactory grainFactory) =>
    {
        var statusGrain = grainFactory.GetGrain<IStatusGrain>(Guid.Empty);
        return statusGrain.GetStatus();
    });


// Map the SignalR Hub
app.MapHub<TickerHub>("/ticker");


// Run the web application
await app.RunAsync();
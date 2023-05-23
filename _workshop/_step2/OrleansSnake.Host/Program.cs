using OrleansSnake.Host.Helpers;
using OrleansSnake.Host.Hubs;
using OrleansSnake.Host.Workers;


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

var app = builder.Build();


// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// Map the SignalR Hub
app.MapHub<TickerHub>("/ticker");


// Run the web application
await app.RunAsync();
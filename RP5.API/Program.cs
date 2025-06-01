using RP5.API.Extensions;
using RP5.API.Models;
using RP5.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MqttBrokerOptions>(builder.Configuration.GetSection("MqttBroker"));
builder.Services.Configure<InfluxDbOptions>(builder.Configuration.GetSection("InfluxDB"));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IInfluxDbService, InfluxDbService>();
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
builder.Services.AddHostedService<MqttBackgroundService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();

app.AddTelemetryApi();

app.Run();
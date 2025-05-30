using Microsoft.Extensions.Options;
using System.Text.Json;
using MQTTnet;
using RP5.API.Models;

namespace RP5.API.Services;

public class MqttBackgroundService : BackgroundService, IAsyncDisposable
{
    private readonly ILogger<MqttBackgroundService> _logger;
    private readonly IMqttClientService _mqttClientService;
    private readonly IInfluxDbService _influxDbService;
    private readonly IOptionsMonitor<MqttBrokerOptions> _mqttBrokerOptions;

    public MqttBackgroundService(
        ILogger<MqttBackgroundService> logger, 
        IMqttClientService mqttClientService, 
        IInfluxDbService influxDbService,
        IOptionsMonitor<MqttBrokerOptions> mqttBrokerOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mqttClientService = mqttClientService ?? throw new ArgumentNullException(nameof(mqttClientService));
        _influxDbService = influxDbService ?? throw new ArgumentNullException(nameof(influxDbService));
        _mqttBrokerOptions = mqttBrokerOptions ?? throw new ArgumentNullException(nameof(mqttBrokerOptions));
        _mqttClientService.OnMessageReceivedAsync += OnMessageReceivedAsync;
    }

    private async Task OnMessageReceivedAsync(string topic, string payload)
    {
        _logger.LogInformation($"Message received on topic {topic}: {payload}");
        
        string[] telemetryTopics = _mqttBrokerOptions.CurrentValue.TelemetryTopics ?? [];
        if (telemetryTopics.Any(e => MqttTopicFilterComparer.Compare(topic, e) == MqttTopicFilterCompareResult.IsMatch))
        {
            _logger.LogDebug("Telemetry topic matched: {Topic}", topic);
            TelemetryData? telemetryData = JsonSerializer.Deserialize<TelemetryData>(payload);
            if (telemetryData is { Temperature: not null, Humidity: not null, Pressure: not null })
            {
                telemetryData.Device = topic.Split('/').LastOrDefault();

                try
                {
                    await _influxDbService.WriteTelemetryDataAsync(telemetryData);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error occurred while writing telemetry data");
                    // We don't want the program to crash in case of a write error, NOM NOM
                }
            }
            else
            {
                _logger.LogWarning("Telemetry data is null or empty");
            }
        }
        else
        {
            _logger.LogWarning("Unhandled topic: {Topic} with payload: {Payload}", topic, payload);   
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _mqttClientService.ConnectAsync(stoppingToken);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        // TODO release managed resources here
        await _mqttClientService.DisconnectAsync(CancellationToken.None);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }
}
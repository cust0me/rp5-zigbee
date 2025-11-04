using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Options;
using RP5.API.Models;

namespace RP5.API.Services;

public interface IInfluxDbService
{
    Task WriteTelemetryDataAsync(TelemetryData telemetryData, CancellationToken cancellationToken = default);
    Task<List<TelemetryData>> GetAsync(string? device, DateTime? start, DateTime? end, CancellationToken cancellationToken = default);
    Task<TelemetryData?> GetLatestAsync(string? device, CancellationToken cancellationToken = default);
    Task<List<string>> GetDevicesAsync(CancellationToken cancellationToken = default);
} 

public sealed class InfluxDbService : IInfluxDbService, IDisposable
{
    private const string measurementName = "telemetry";

    private readonly ILogger<InfluxDbService> _logger;
    
    private bool _disposedValue;

    private readonly InfluxDBClient _client;

    private readonly string _orgId;
    private readonly string _bucket;

    public InfluxDbService(ILogger<InfluxDbService> logger, IOptions<InfluxDbOptions> influxDbOptions)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(influxDbOptions, nameof(influxDbOptions));
        ArgumentNullException.ThrowIfNull(influxDbOptions.Value.Host, nameof(influxDbOptions.Value.Host));
        ArgumentNullException.ThrowIfNull(influxDbOptions.Value.Token, nameof(influxDbOptions.Value.Token));
        ArgumentNullException.ThrowIfNull(influxDbOptions.Value.OrgId, nameof(influxDbOptions.Value.OrgId));
        ArgumentNullException.ThrowIfNull(influxDbOptions.Value.Bucket, nameof(influxDbOptions.Value.Bucket));

        _logger = logger;
        string hostUrl = influxDbOptions.Value.Host;
        string authToken = influxDbOptions.Value.Token;
        _orgId = influxDbOptions.Value.OrgId;
        _bucket = influxDbOptions.Value.Bucket;
        
        _client = new InfluxDBClient(hostUrl, authToken);
    }
    
    public async Task WriteTelemetryDataAsync(TelemetryData telemetryData, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Writing telemetry data to InfluxDB");
        try
        {
            WriteApiAsync writeApi = _client.GetWriteApiAsync();

            PointData point = PointData.Measurement(measurementName)
                .Field("temperature", telemetryData.Temperature)
                .Field("humidity", telemetryData.Humidity)
                .Field("pressure", telemetryData.Pressure)
                .Field("device", telemetryData.Device)
                .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            await writeApi.WritePointAsync(point, _bucket, _orgId, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while writing telemetry data to InfluxDB");
            throw;
        }
    }

    public async Task<List<TelemetryData>> GetAsync(string? device, DateTime? start, DateTime? end, CancellationToken cancellationToken)
    {   
        // Null assignment operators to set default values
        start ??= DateTime.UtcNow.AddHours(-1);
        end ??= DateTime.UtcNow;
        
        _logger.LogInformation("Getting telemetry data from InfluxDB for device:{device} in the range: {start} - {end}", device ?? "null", start, end);
        try
        {
            QueryApi queryApiAsync = _client.GetQueryApi();

            string fluxQuery = 
            $"""
            from(bucket: "{_bucket}")
                // We want to make sure that the data we query is in the given time range
                |> range(start: {start:yyyy-MM-ddTHH:mm:ssZ}, stop: {end:yyyy-MM-ddTHH:mm:ssZ})

                // We only want to query the telemetry data
                |> filter(fn: (r) => r._measurement == "{measurementName}")
                
                // groups the rows by time, and transposes to single row
                |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
                
                // we only want to keep a few select columns
                |> keep(columns: ["_time", "device", "humidity", "temperature", "pressure"])

                // sort by time
                |> sort(columns: ["_time"], desc: false)
            
                // null check
                |> filter(fn: (r) => exists r.device and exists r.temperature and exists r.humidity and exists r.pressure)
            """;

            if (string.IsNullOrWhiteSpace(device) == false)
            {
                fluxQuery += 
                    $"""
                    // We only want to query the telemetry data for the given device
                    |> filter(fn: (r) => r.device == "{device}")
                    """;
            }

            List<TelemetryData> result = await queryApiAsync.QueryAsync<TelemetryData>(fluxQuery, _orgId, cancellationToken: cancellationToken);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while reading historical telemetry data from InfluxDB");
            throw;
        }
    }
    
    public async Task<TelemetryData?> GetLatestAsync(string? device, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting latest telemetry data for device:{device} from InfluxDB", device ?? "null");
        try
        {
            QueryApi queryApiAsync = _client.GetQueryApi();

            string fluxQuery = 
            $"""
            from(bucket: "{_bucket}")
                // We want to make sure that the data we query is in the given time range
                |> range(start: -30d)

                // We only want to query the telemetry data
                |> filter(fn: (r) => r._measurement == "{measurementName}")
                
                // groups the rows by time, and transposes to single row
                |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
                
                // we only want to keep a few select columns
                |> keep(columns: ["_time", "device", "humidity", "temperature", "pressure"])

                // null check
                |> filter(fn: (r) => exists r.device and exists r.temperature and exists r.humidity and exists r.pressure)

                // sort by time
                |> sort(columns: ["_time"], desc: true)
            """;
            
            if (string.IsNullOrWhiteSpace(device) == false)
            {
                fluxQuery += 
                $"""
                     // We only want to query the telemetry data for the given device
                     |> filter(fn: (r) => r.device == "{device}")
                 """;
            }

            fluxQuery +=
            """
                // only take the first element
                |> top(n: 1)
            """;

            List<TelemetryData> result = await queryApiAsync.QueryAsync<TelemetryData>(fluxQuery, _orgId, cancellationToken: cancellationToken);
            return result.FirstOrDefault();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while reading latest telemetry data from InfluxDB");
            throw;
        }
    }
    
    public async Task<List<string>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting devices from InfluxDB");
        try
        {
            QueryApi queryApiAsync = _client.GetQueryApi();

            string fluxQuery = 
                $"""
                 from(bucket: "{_bucket}")
                     // We want to make sure that the data we query is in the given time range
                     |> range(start: -1d)
                 
                     // We only want to query the telemetry data
                     |> filter(fn: (r) => r._measurement == "{measurementName}")
                     
                     // groups the rows by time, and transposes to single row
                     |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
                 
                     // null check
                     |> filter(fn: (r) => exists r.device)
                     
                     // we only want to keep a few select columns
                     |> keep(columns: ["device"])
                 """;
            
            // Using FluxTable here as we're not directly deserializing to a specific type
            List<FluxTable>? result = await queryApiAsync.QueryAsync(fluxQuery, _orgId, cancellationToken: cancellationToken);
            
            // Select all records from all tables
            List<FluxRecord> records = [.. result.SelectMany(e => e.Records).Where(e => e is not null)];
            
            // Select every device field from the records and ensure distinct values
            List<string> devices = [.. 
                records.Select(record => record.GetValueByKey("device")?.ToString())
                .Where(device => device is not null)
                .Select(e => e!)
                .Distinct()
            ];
            
            return devices;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while retrieving devices from InfluxDB");
            throw;
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _client.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
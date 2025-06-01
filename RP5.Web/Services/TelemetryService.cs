using RP5.Web.Models;
using System.Diagnostics;
using System.Text.Json;

namespace RP5.Web.Services;

public interface ITelemetryService
{
    Task<TelemetryData?> GetLatestTelemetryAsync(string? device, CancellationToken cancellationToken = default);
    Task<List<TelemetryData>> GetHistoricalTelemetryAsync(DateTime start, DateTime end, string? device, CancellationToken cancellationToken = default);
    Task<List<string>> GetDevicesAsync(CancellationToken cancellationToken = default);
}

public class TelemetryService(HttpClient httpClient) : ITelemetryService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<TelemetryData?> GetLatestTelemetryAsync(string? device, CancellationToken cancellationToken)
    {
        try
        {
            return await GetAsync<TelemetryData>($"telemetry/latest{(device is not null ? $"&device={Uri.EscapeDataString(device)}" : "")}", cancellationToken);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return null;
        }
    }

    public async Task<List<TelemetryData>> GetHistoricalTelemetryAsync(DateTime start, DateTime end, string? device, CancellationToken cancellationToken)
    {
        try
        {
            return await GetAsync<List<TelemetryData>>($"telemetry/historical?start={Uri.EscapeDataString(start.ToString("O"))}&end={Uri.EscapeDataString(end.ToString("O"))}{(device is not null ? $"&device={Uri.EscapeDataString(device)}" : "")}", cancellationToken) ?? [];
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return [];
        }
    }

    public async Task<List<string>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await GetAsync<List<string>>("telemetry/devices", cancellationToken) ?? [];
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return [];
        }
    }

    private async Task<T?> GetAsync<T>(string uri, CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(uri, cancellationToken);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content))
                return default;

            return JsonSerializer.Deserialize<T>(content, _jsonOptions)
                ?? throw new InvalidOperationException("Deserialization returned null.");
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"Error during GET request to {uri}", ex);
        }
    }
}
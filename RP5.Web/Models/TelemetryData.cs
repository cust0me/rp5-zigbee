using System.Text.Json.Serialization;

namespace RP5.Web.Models;

public class TelemetryData
{
    [JsonPropertyName("temperature")]
    public float? Temperature { get; init; }

    [JsonPropertyName("humidity")]
    public float? Humidity { get; init; }

    [JsonPropertyName("pressure")]
    public float? Pressure { get; init; }

    [JsonPropertyName("_time")]
    public DateTime? Time { get; init; }

    [JsonPropertyName("device")]
    public string? Device { get; set; }
}
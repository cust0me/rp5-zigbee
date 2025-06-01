using Microsoft.AspNetCore.Components;
using RP5.Web.Models;
using RP5.Web.Services;
using Syncfusion.Blazor.Charts;

namespace RP5.Web.Components.Components;

public partial class TelemetryComponent(ITelemetryService telemetryService) : ComponentBase
{
    private readonly ITelemetryService _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));

    [Parameter, EditorRequired]
    public List<string>? Devices { get; set; }

    private DateTime? _startDate = DateTime.UtcNow.AddDays(-1);

    private DateTime? _endDate = DateTime.UtcNow;
    private string? SelectedDevice { get; set; } = null;
    private string? DateTimeFormat { get; set; } = "HH:mm";

    private List<TelemetryData> _telemetryData = [];

    private IEnumerable<TelemetryData> FilteredTelemetryData => _telemetryData;

    private float MinTemp => _telemetryData.Count > 0 ? (float)(Math.Floor(_telemetryData.Min(e => e.Temperature) ?? 15f) - 2f) : 0f;

    private float MaxTemp => _telemetryData.Count > 0 ? (float)(Math.Ceiling(_telemetryData.Max(e => e.Temperature) ?? 25f) + 2f) : 40f;

    private float MinHum => _telemetryData.Count > 0 ? (float)(Math.Floor(_telemetryData.Min(e => e.Humidity) ?? 55f) - 2f) : 55f;

    private float MaxHum => _telemetryData.Count > 0 ? (float)(Math.Ceiling(_telemetryData.Max(e => e.Humidity) ?? 80f) + 2f) : 80f;


    private async Task LoadDataAsync()
    {
        try
        {
            if (_startDate is not null && _endDate is not null)
            {
                _telemetryData = await _telemetryService.GetHistoricalTelemetryAsync(_startDate.Value, _endDate.Value, SelectedDevice);
            }
        }
        catch
        {
            _telemetryData = [];
        }
        finally
        {
            StateHasChanged();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        // Set default dates if not set
        _startDate ??= DateTime.UtcNow.AddDays(-1);
        _endDate ??= DateTime.UtcNow;

        SetSelectedDevice(string.IsNullOrEmpty(SelectedDevice) ? null : SelectedDevice);

        // Load data with current parameters
        if (_startDate.HasValue && _endDate.HasValue)
        {
            await LoadDataAsync();
        }
    }

    private async Task DeviceChangedCallback(ChangeEventArgs obj)
    {
        string? selected = obj.Value?.ToString();
        SetSelectedDevice(selected);
        await LoadDataAsync();
    }

    private async Task StartChangedCallback(ChangeEventArgs obj)
    {
        if (DateTime.TryParse((string?)obj.Value, out DateTime dateTime))
        {
            _startDate = dateTime;

            DateTime start = _startDate.Value;
            DateTime end = _endDate ?? DateTime.UtcNow;

            DateTimeFormat = end.Subtract(start) <= TimeSpan.FromDays(1) ? "HH:mm" : "dd MMM";

            await LoadDataAsync();
        }
    }

    private async Task EndChangedCallback(ChangeEventArgs obj)
    {
        if (DateTime.TryParse((string?)obj.Value, out DateTime dateTime))
        {
            _endDate = dateTime;

            DateTime start = _startDate ?? DateTime.UtcNow.AddDays(-1);
            DateTime end = _endDate.Value;

            DateTimeFormat = end.Subtract(start) <= TimeSpan.FromDays(1) ? "HH:mm" : "dd MMM";

            await LoadDataAsync();
        }
    }

    private void SetSelectedDevice(string? device)
    {
        SelectedDevice = device == "All" ? null : device;
    }
}
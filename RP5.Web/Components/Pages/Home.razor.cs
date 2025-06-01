using RP5.Web.Services;

namespace RP5.Web.Components.Pages;

public partial class Home(ITelemetryService telemetry)
{
    private readonly ITelemetryService _telemetryService = telemetry ?? throw new ArgumentNullException(nameof(telemetry));


    private List<string> _devices = [];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _devices = ["All", .. await _telemetryService.GetDevicesAsync()];
    }
}
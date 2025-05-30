using Microsoft.AspNetCore.Mvc;
using RP5.API.Models;
using RP5.API.Services;

namespace RP5.API.Extensions;

public static class TelemetryApiExtensions
{
    public static void AddTelemetryApi(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("telemetry")
            .WithTags("Telemetry")
            .WithOpenApi();
        
        group.MapGet("latest", async (
                [FromServices] IInfluxDbService influxDbService,
                string? device,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    TelemetryData? telemetryData = await influxDbService.GetLatestAsync(device, cancellationToken);
                    return Results.Ok(telemetryData);
                }
                catch (Exception e)
                {
                    app.Logger.LogError(e, "An error occurred while getting telemetry latest data");
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);
                }
            })
            .Produces<TelemetryData?>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("historical", async (
                [FromServices] IInfluxDbService influxDbService,
                string? device,
                DateTime? start,
                DateTime? end,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    List<TelemetryData> telemetryData = await influxDbService.GetAsync(device, start, end, cancellationToken);
                    return Results.Ok(telemetryData);
                }
                catch (Exception e)
                {
                    app.Logger.LogError(e, "An error occurred while getting telemetry data");
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);
                }
            })
            .Produces<List<TelemetryData>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("devices", async (
                [FromServices] IInfluxDbService influxDbService, 
                CancellationToken cancellationToken) =>
        {
            try
            {
                List<string> results = await influxDbService.GetDevicesAsync(cancellationToken);
                return Results.Ok(results);
            }
            catch (Exception e)
            {
                app.Logger.LogError(e, "An error occurred while getting devices");
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        })
        .Produces<List<string>>()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}
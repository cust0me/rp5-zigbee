using RP5.Web.Services;

namespace RP5.Web.Extensions;

public static class ApiServiceExtensions
{
    public static void AddApiService(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddHttpContextAccessor();

        serviceCollection
            .AddHttpClient<ITelemetryService, TelemetryService>(opt =>
            {
                opt.BaseAddress = new Uri(configuration["ExternalAPI"] ?? "http://localhost:5001");
            });
    }
}
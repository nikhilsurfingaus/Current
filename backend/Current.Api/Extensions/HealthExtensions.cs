using Current.Api.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Current.Api.Extensions;

public static class HealthExtensions
{
    public static IServiceCollection AddHealthMonitoring(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(
                name: "database",
                failureStatus: HealthStatus.Unhealthy);

        return services;
    }

    public static WebApplication MapHealthMonitoring(this WebApplication app)
    {
        app.MapHealthChecks("/health");

        return app;
    }
}

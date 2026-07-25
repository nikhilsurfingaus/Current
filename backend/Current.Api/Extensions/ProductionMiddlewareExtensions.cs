using Current.Api.Middleware;
using Microsoft.AspNetCore.HttpOverrides;

namespace Current.Api.Extensions;

public static class ProductionMiddlewareExtensions
{
    public static IServiceCollection AddProductionMiddleware(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }

    public static WebApplication UseProductionMiddleware(this WebApplication app)
    {
        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseForwardedHeaders();

            if (app.Environment.IsProduction())
            {
                app.UseHsts();
            }
        }

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseSecurityHeaders();

        return app;
    }

    private static WebApplication UseSecurityHeaders(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var responseHeaders = context.Response.Headers;
            responseHeaders["X-Content-Type-Options"] = "nosniff";
            responseHeaders["X-Frame-Options"] = "DENY";
            responseHeaders["Referrer-Policy"] = "strict-origin-when-cross-origin";
            responseHeaders["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

            await next();
        });

        return app;
    }
}

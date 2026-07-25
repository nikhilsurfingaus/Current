using Current.Api.Configuration;

namespace Current.Api.Extensions;

public static class CorsExtensions
{
    public const string FrontendPolicy = "Frontend";

    public static IServiceCollection AddFrontendCors(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.Configure<CorsOptions>(configuration.GetSection(CorsOptions.SectionName));

        services.AddCors(options =>
        {
            options.AddPolicy(FrontendPolicy, policy =>
            {
                if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
                {
                    policy.SetIsOriginAllowed(origin =>
                            Uri.TryCreate(origin, UriKind.Absolute, out var originUri) &&
                            originUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
                else
                {
                    var corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>();
                    var allowedOrigins = corsOptions?.AllowedOrigins
                        .Where(origin => !string.IsNullOrWhiteSpace(origin))
                        .Select(origin => origin.Trim())
                        .ToArray() ?? [];

                    if (allowedOrigins.Length == 0)
                    {
                        throw new InvalidOperationException(
                            "Cors:AllowedOrigins must contain at least one origin in non-development environments.");
                    }

                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        return services;
    }
}

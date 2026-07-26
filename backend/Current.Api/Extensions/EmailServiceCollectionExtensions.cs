using Current.Api.Configuration;
using Current.Api.Interfaces;
using Current.Api.Services;
using Current.Api.Services.Email;

namespace Current.Api.Extensions;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.AddScoped<IEmailVerificationService, EmailVerificationService>();

        if (environment.IsEnvironment("Testing"))
        {
            services.AddSingleton<CapturingEmailSender>();
            services.AddSingleton<IEmailSender>(serviceProvider =>
                serviceProvider.GetRequiredService<CapturingEmailSender>());
            return services;
        }

        var emailOptions = configuration.GetSection(EmailOptions.SectionName).Get<EmailOptions>();

        if (emailOptions is not null && emailOptions.IsResendApiConfigured())
        {
            services.AddHttpClient<ResendEmailSender>();
            services.AddSingleton<IEmailSender>(serviceProvider =>
                serviceProvider.GetRequiredService<ResendEmailSender>());
            return services;
        }

        if (emailOptions is not null && emailOptions.IsSmtpConfigured())
        {
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
            return services;
        }

        services.AddSingleton<IEmailSender, LoggingEmailSender>();
        return services;
    }
}

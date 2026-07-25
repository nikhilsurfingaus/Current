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
        var smtpConfigured = emailOptions is not null &&
                             emailOptions.Enabled &&
                             !string.IsNullOrWhiteSpace(emailOptions.SmtpHost);

        if (smtpConfigured)
        {
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
        }
        else
        {
            services.AddSingleton<IEmailSender, LoggingEmailSender>();
        }

        return services;
    }
}

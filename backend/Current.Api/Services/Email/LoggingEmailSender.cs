using Current.Api.Interfaces;

namespace Current.Api.Services.Email;

public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendVerificationCodeAsync(string recipientEmail, string verificationCode, DateTime expiresAtUtc)
    {
        _logger.LogInformation(
            "Email verification code for {Email}: {VerificationCode} (expires {ExpiresAtUtc:u})",
            recipientEmail,
            verificationCode,
            expiresAtUtc);

        return Task.CompletedTask;
    }
}

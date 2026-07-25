using System.Net;
using System.Net.Mail;
using Current.Api.Configuration;
using Current.Api.Interfaces;
using Microsoft.Extensions.Options;

namespace Current.Api.Services.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailOptions> emailOptions, ILogger<SmtpEmailSender> logger)
    {
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    public async Task SendVerificationCodeAsync(string recipientEmail, string verificationCode, DateTime expiresAtUtc)
    {
        using var smtpClient = new SmtpClient(_emailOptions.SmtpHost, _emailOptions.SmtpPort)
        {
            EnableSsl = _emailOptions.UseStartTls,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
        };

        if (!string.IsNullOrWhiteSpace(_emailOptions.SmtpUsername))
        {
            smtpClient.Credentials = new NetworkCredential(
                _emailOptions.SmtpUsername,
                _emailOptions.SmtpPassword);
        }

        var fromAddress = new MailAddress(_emailOptions.FromAddress, _emailOptions.FromName);
        using var mailMessage = new MailMessage
        {
            From = fromAddress,
            Subject = "Verify your Current account",
            Body = $"""
                Your Current verification code is: {verificationCode}

                This code expires at {expiresAtUtc:u}.

                If you did not create an account, you can ignore this email.
                """,
            IsBodyHtml = false,
        };

        mailMessage.To.Add(recipientEmail);

        try
        {
            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to send verification email to {Email}", recipientEmail);
            throw new InvalidOperationException("Unable to send verification email. Please try again later.");
        }
    }
}

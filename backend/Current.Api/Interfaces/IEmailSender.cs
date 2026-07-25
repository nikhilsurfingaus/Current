namespace Current.Api.Interfaces;

public interface IEmailSender
{
    Task SendVerificationCodeAsync(string recipientEmail, string verificationCode, DateTime expiresAtUtc);
}

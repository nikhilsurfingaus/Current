using System.Collections.Concurrent;
using Current.Api.Interfaces;

namespace Current.Api.Services.Email;

public class CapturingEmailSender : IEmailSender
{
    private readonly ConcurrentDictionary<string, string> _verificationCodesByEmail = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, string> VerificationCodesByEmail => _verificationCodesByEmail;

    public Task SendVerificationCodeAsync(string recipientEmail, string verificationCode, DateTime expiresAtUtc)
    {
        _verificationCodesByEmail[recipientEmail.Trim().ToLowerInvariant()] = verificationCode;
        return Task.CompletedTask;
    }

    public void Clear()
    {
        _verificationCodesByEmail.Clear();
    }
}

namespace Current.Api.Configuration;

public class EmailOptions
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; }

    public string FromAddress { get; set; } = string.Empty;

    public string FromName { get; set; } = string.Empty;

    /// <summary>
    /// Resend API key (re_...). Preferred on Render — SMTP ports are blocked.
    /// </summary>
    public string? ApiKey { get; set; }

    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; }

    public string? SmtpUsername { get; set; }

    public string? SmtpPassword { get; set; }

    public bool UseStartTls { get; set; } = true;

    public string? ResolveResendApiKey()
    {
        if (!string.IsNullOrWhiteSpace(ApiKey))
        {
            return ApiKey.Trim();
        }

        if (SmtpHost.Contains("resend", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(SmtpPassword))
        {
            return SmtpPassword.Trim();
        }

        return null;
    }

    public bool IsResendApiConfigured()
    {
        return Enabled && !string.IsNullOrWhiteSpace(ResolveResendApiKey());
    }

    public bool IsSmtpConfigured()
    {
        return Enabled && !string.IsNullOrWhiteSpace(SmtpHost);
    }
}

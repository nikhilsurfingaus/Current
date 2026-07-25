namespace Current.Api.Configuration;

public class EmailOptions
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; }

    public string FromAddress { get; set; } = string.Empty;

    public string FromName { get; set; } = string.Empty;

    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; }

    public string? SmtpUsername { get; set; }

    public string? SmtpPassword { get; set; }

    public bool UseStartTls { get; set; } = true;
}

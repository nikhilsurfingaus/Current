namespace Current.Api.Configuration;

public class CorsOptions
{
    public const string SectionName = "Cors";

    public List<string> AllowedOrigins { get; set; } = [];
}

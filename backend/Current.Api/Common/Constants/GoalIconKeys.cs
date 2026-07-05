namespace Current.Api.Common.Constants;

public static class GoalIconKeys
{
    public const string Default = "default";
    public const string Vacation = "vacation";
    public const string Home = "home";
    public const string Emergency = "emergency";
    public const string Car = "car";
    public const string Gaming = "gaming";
    public const string Investment = "investment";
    public const string Education = "education";

    private static readonly HashSet<string> AllowedKeys = new(StringComparer.Ordinal)
    {
        Default,
        Vacation,
        Home,
        Emergency,
        Car,
        Gaming,
        Investment,
        Education,
    };

    public static string Normalize(string? iconKey)
    {
        if (string.IsNullOrWhiteSpace(iconKey))
        {
            return Default;
        }

        var normalizedIconKey = iconKey.Trim().ToLowerInvariant();
        return AllowedKeys.Contains(normalizedIconKey) ? normalizedIconKey : Default;
    }
}

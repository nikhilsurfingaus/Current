using Current.Api.Common.Enums;

namespace Current.Api.DTOs.Users;

public class UpdateUserPreferencesRequest
{
    public ThemePreference ThemePreference { get; set; }

    public string PreferredCurrency { get; set; } = string.Empty;

    public string Timezone { get; set; } = string.Empty;

    public string Locale { get; set; } = string.Empty;
}

using Current.Api.Common.Enums;

namespace Current.Api.DTOs.Users;

public class UserResponse
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public ThemePreference ThemePreference { get; set; }

    public string PreferredCurrency { get; set; } = string.Empty;

    public string Timezone { get; set; } = string.Empty;

    public string Locale { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

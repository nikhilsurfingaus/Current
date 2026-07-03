using Current.Api.Common.Enums;

namespace Current.Api.DTOs.Auth;

public class AuthResponse
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}

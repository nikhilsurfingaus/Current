namespace Current.Api.DTOs.Auth;

public class RegisterResponse
{
    public string Email { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime VerificationExpiresAt { get; set; }
}

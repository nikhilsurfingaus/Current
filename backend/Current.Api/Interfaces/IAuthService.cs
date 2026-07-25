using Current.Api.DTOs.Auth;

namespace Current.Api.Interfaces;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);

    Task<AuthResponse> VerifyEmailAsync(VerifyEmailRequest request);

    Task<RegisterResponse> ResendVerificationAsync(ResendVerificationRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);
}

using Current.Api.DTOs.Auth;

namespace Current.Api.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);
}

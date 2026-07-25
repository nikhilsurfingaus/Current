using Current.Api.DTOs.Auth;
using Current.Api.Entities;

namespace Current.Api.Interfaces;

public interface IEmailVerificationService
{
    Task<RegisterResponse> BeginRegistrationAsync(RegisterRequest request);

    Task<User> VerifyEmailAsync(VerifyEmailRequest request);

    Task<RegisterResponse> ResendVerificationAsync(ResendVerificationRequest request);
}

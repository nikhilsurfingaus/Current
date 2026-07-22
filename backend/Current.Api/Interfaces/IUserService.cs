using Current.Api.DTOs.Users;

namespace Current.Api.Interfaces;

public interface IUserService
{
    Task<UserResponse?> GetUserByIdAsync(Guid userId, Guid currentUserId);

    Task<UserResponse?> UpdateProfileAsync(
        Guid currentUserId,
        UpdateUserProfileRequest request);

    Task<UserResponse?> UpdatePreferencesAsync(
        Guid currentUserId,
        UpdateUserPreferencesRequest request);
}

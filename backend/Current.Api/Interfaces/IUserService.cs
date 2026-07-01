using Current.Api.DTOs.Users;

namespace Current.Api.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllUsersAsync();

    Task<UserResponse?> GetUserByIdAsync(Guid userId);

    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
}

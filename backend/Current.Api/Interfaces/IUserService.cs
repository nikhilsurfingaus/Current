using Current.Api.DTOs.Users;

namespace Current.Api.Interfaces;

// Contract for user business logic — keeps controllers decoupled from EF Core
public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllUsersAsync();

    Task<UserResponse?> GetUserByIdAsync(Guid userId);

    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
}

using Current.Api.DTOs.Users;

namespace Current.Api.Interfaces;

public interface IUserService
{
    Task<UserResponse?> GetUserByIdAsync(Guid userId, Guid currentUserId);
}

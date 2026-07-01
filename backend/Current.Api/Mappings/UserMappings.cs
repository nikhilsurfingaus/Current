using Current.Api.DTOs.Users;
using Current.Api.Entities;

namespace Current.Api.Mappings;

// Converts database entities → API response DTOs (never expose entities directly)
public static class UserMappings
{
    public static UserResponse ToResponse(this User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}

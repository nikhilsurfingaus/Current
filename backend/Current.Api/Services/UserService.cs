using Current.Api.Data;
using Current.Api.DTOs.Users;
using Current.Api.Interfaces;
using Current.Api.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;

    public UserService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid userId, Guid currentUserId)
    {
        if (userId != currentUserId)
        {
            return null;
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == userId);

        return user?.ToResponse();
    }
}

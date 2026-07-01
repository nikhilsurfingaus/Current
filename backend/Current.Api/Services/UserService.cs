using Current.Api.Data;
using Current.Api.DTOs.Users;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Current.Api.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;

    // DbContext is injected automatically by the DI container (see ServiceCollectionExtensions)
    public UserService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllUsersAsync()
    {
        var users = await _dbContext.Users
            .AsNoTracking() // read-only — no change tracking overhead on GET
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ToListAsync();

        return users.Select(user => user.ToResponse()).ToList();
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid userId)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == userId);

        return user?.ToResponse();
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        var emailAlreadyExists = await _dbContext.Users
            .AnyAsync(user => user.Email == request.Email);

        if (emailAlreadyExists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var utcNow = DateTime.UtcNow;

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(); // commits to PostgreSQL

        return user.ToResponse();
    }
}

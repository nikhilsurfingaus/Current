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

    public async Task<UserResponse?> UpdateProfileAsync(
        Guid currentUserId,
        UpdateUserProfileRequest request)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == currentUserId);

        if (user is null)
        {
            return null;
        }

        var firstName = NormalizeName(request.FirstName);
        var lastName = NormalizeName(request.LastName);

        user.FirstName = firstName;
        user.LastName = lastName;
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return user.ToResponse();
    }

    public async Task<UserResponse?> UpdatePreferencesAsync(
        Guid currentUserId,
        UpdateUserPreferencesRequest request)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == currentUserId);

        if (user is null)
        {
            return null;
        }

        user.ThemePreference = request.ThemePreference;
        user.PreferredCurrency = NormalizeCurrency(request.PreferredCurrency);
        user.Timezone = NormalizeTimezone(request.Timezone);
        user.Locale = NormalizeLocale(request.Locale);
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return user.ToResponse();
    }

    private static string NormalizeName(string name)
    {
        var normalizedName = name.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new InvalidOperationException("Name is required.");
        }

        if (normalizedName.Length > 100)
        {
            throw new InvalidOperationException("Name must be 100 characters or fewer.");
        }

        return normalizedName;
    }

    private static string NormalizeCurrency(string currency)
    {
        var normalizedCurrency = currency.Trim().ToUpperInvariant();

        if (normalizedCurrency.Length != 3)
        {
            throw new InvalidOperationException("Currency must be a 3-letter code.");
        }

        return normalizedCurrency;
    }

    private static string NormalizeTimezone(string timezone)
    {
        var normalizedTimezone = timezone.Trim();

        if (string.IsNullOrWhiteSpace(normalizedTimezone))
        {
            throw new InvalidOperationException("Timezone is required.");
        }

        if (normalizedTimezone.Length > 100)
        {
            throw new InvalidOperationException("Timezone must be 100 characters or fewer.");
        }

        return normalizedTimezone;
    }

    private static string NormalizeLocale(string locale)
    {
        var normalizedLocale = locale.Trim();

        if (string.IsNullOrWhiteSpace(normalizedLocale))
        {
            throw new InvalidOperationException("Locale is required.");
        }

        if (normalizedLocale.Length > 20)
        {
            throw new InvalidOperationException("Locale must be 20 characters or fewer.");
        }

        return normalizedLocale;
    }
}

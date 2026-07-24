using Current.Api.Common.Constants;
using Current.Api.Common.Enums;
using Current.Api.Data;
using Current.Api.Entities;
using Microsoft.AspNetCore.Identity;

namespace Current.Api.Tests.Helpers;

public static class TestDataSeeder
{
    public static async Task<User> SeedUserAsync(
        ApplicationDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        string firstName,
        string lastName,
        string email,
        string password,
        UserRole role = UserRole.User)
    {
        var utcNow = DateTime.UtcNow;
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var userToCreate = new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = normalizedEmail,
            Role = role,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
        };

        userToCreate.PasswordHash = passwordHasher.HashPassword(userToCreate, password);

        dbContext.Users.Add(userToCreate);
        await dbContext.SaveChangesAsync();

        return userToCreate;
    }

    public static async Task<Account> SeedAccountAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        string name,
        AccountType accountType,
        decimal currentBalance,
        string currency = "AUD")
    {
        var utcNow = DateTime.UtcNow;
        var accountToCreate = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name.Trim(),
            AccountType = accountType,
            CurrentBalance = currentBalance,
            Currency = currency,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
        };

        dbContext.Accounts.Add(accountToCreate);
        await dbContext.SaveChangesAsync();

        return accountToCreate;
    }

    public static async Task<(Branch Branch, Account TreasuryAccount)> SeedBranchTreasuryAsync(
        ApplicationDbContext dbContext,
        decimal treasuryBalance = 10_000_000m)
    {
        var utcNow = DateTime.UtcNow;
        var systemUser = new User
        {
            Id = BranchConstants.SystemUserId,
            FirstName = "Current",
            LastName = "Branch",
            Email = BranchConstants.SystemUserEmail,
            PasswordHash = "SYSTEM_NO_LOGIN",
            Role = UserRole.Admin,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
        };

        var treasuryAccount = new Account
        {
            Id = BranchConstants.HqTreasuryAccountId,
            UserId = BranchConstants.SystemUserId,
            Name = BranchConstants.HqTreasuryAccountName,
            AccountType = AccountType.Branch,
            CurrentBalance = treasuryBalance,
            Currency = "AUD",
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
        };

        var branchToCreate = new Branch
        {
            Id = BranchConstants.HqBranchId,
            Name = BranchConstants.HqBranchName,
            Code = BranchConstants.HqBranchCode,
            TreasuryAccountId = BranchConstants.HqTreasuryAccountId,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
        };

        dbContext.Users.Add(systemUser);
        dbContext.Accounts.Add(treasuryAccount);
        dbContext.Branches.Add(branchToCreate);
        await dbContext.SaveChangesAsync();

        return (branchToCreate, treasuryAccount);
    }

    public static async Task<Notification> SeedNotificationAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        NotificationType notificationType,
        string title,
        string body,
        bool isRead = false,
        Guid? relatedEntityId = null,
        DateTime? createdAt = null)
    {
        var utcNow = createdAt ?? DateTime.UtcNow;
        var notificationToCreate = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Body = body,
            NotificationType = notificationType,
            RelatedEntityId = relatedEntityId,
            IsRead = isRead,
            CreatedAt = utcNow,
        };

        dbContext.Notifications.Add(notificationToCreate);
        await dbContext.SaveChangesAsync();

        return notificationToCreate;
    }
}

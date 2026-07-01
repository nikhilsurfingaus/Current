using Current.Api.Data;
using Current.Api.DTOs.Accounts;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Current.Api.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Services;

public class AccountService : IAccountService
{
    private readonly ApplicationDbContext _dbContext;

    public AccountService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AccountResponse>> GetAllAccountsAsync()
    {
        var accounts = await _dbContext.Accounts
            .AsNoTracking()
            .OrderBy(account => account.Name)
            .ToListAsync();

        return accounts.Select(account => account.ToResponse()).ToList();
    }

    public async Task<AccountResponse?> GetAccountByIdAsync(Guid accountId)
    {
        var account = await _dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(account => account.Id == accountId);

        return account?.ToResponse();
    }

    public async Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request)
    {
        // Account must belong to an existing user
        var userExists = await _dbContext.Users
            .AnyAsync(user => user.Id == request.UserId);

        if (!userExists)
        {
            throw new InvalidOperationException("User not found.");
        }

        var utcNow = DateTime.UtcNow;

        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Name = request.Name.Trim(),
            AccountType = request.AccountType,
            CurrentBalance = request.CurrentBalance,
            Currency = request.Currency.Trim().ToUpperInvariant(), // e.g. "usd" → "USD"
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        return account.ToResponse();
    }
}

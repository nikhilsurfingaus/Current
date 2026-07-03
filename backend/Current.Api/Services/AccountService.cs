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

    public async Task<IReadOnlyList<AccountResponse>> GetAllAccountsAsync(Guid currentUserId)
    {
        var accounts = await _dbContext.Accounts
            .AsNoTracking()
            .Where(account => account.UserId == currentUserId)
            .OrderBy(account => account.Name)
            .ToListAsync();

        return accounts.Select(account => account.ToResponse()).ToList();
    }

    public async Task<AccountResponse?> GetAccountByIdAsync(Guid accountId, Guid currentUserId)
    {
        var account = await _dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(account =>
                account.Id == accountId && account.UserId == currentUserId);

        return account?.ToResponse();
    }

    public async Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request, Guid currentUserId)
    {
        var utcNow = DateTime.UtcNow;

        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Name = request.Name.Trim(),
            AccountType = request.AccountType,
            CurrentBalance = request.CurrentBalance,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        return account.ToResponse();
    }
}

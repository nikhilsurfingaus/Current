using Current.Api.Common.Constants;
using Current.Api.Common.Enums;
using Current.Api.Data;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Services;

public class DisbursementService : IDisbursementService
{
    private readonly ApplicationDbContext _dbContext;

    public DisbursementService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Branch> GetDefaultBranchAsync()
    {
        var branch = await _dbContext.Branches
            .Include(branch => branch.TreasuryAccount)
            .FirstOrDefaultAsync(branch => branch.Id == BranchConstants.HqBranchId);

        if (branch is null)
        {
            throw new InvalidOperationException("Default branch is not configured.");
        }

        return branch;
    }

    public Task ApplyDisbursementAsync(
        Account treasuryAccount,
        Account recipientAccount,
        decimal amount,
        string description,
        TransactionCategory category)
    {
        if (amount <= 0)
        {
            throw new InvalidOperationException("Disbursement amount must be greater than zero.");
        }

        if (!string.Equals(treasuryAccount.Currency, recipientAccount.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Treasury and recipient account currencies must match.");
        }

        if (treasuryAccount.CurrentBalance < amount)
        {
            throw new InvalidOperationException("Branch treasury has insufficient funds.");
        }

        var utcNow = DateTime.UtcNow;
        var normalizedDescription = description.Trim();

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            FromAccountId = treasuryAccount.Id,
            ToAccountId = recipientAccount.Id,
            Amount = amount,
            Description = normalizedDescription,
            Category = category,
            Reference = $"BRANCH-{utcNow:yyyyMMddHHmmss}",
            Status = TransactionStatus.Completed,
            CreatedAt = utcNow
        };

        var debitEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            TransactionId = transaction.Id,
            AccountId = treasuryAccount.Id,
            EntryType = LedgerEntryType.Debit,
            Amount = amount,
            CreatedAt = utcNow
        };

        var creditEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            TransactionId = transaction.Id,
            AccountId = recipientAccount.Id,
            EntryType = LedgerEntryType.Credit,
            Amount = amount,
            CreatedAt = utcNow
        };

        treasuryAccount.CurrentBalance -= amount;
        treasuryAccount.UpdatedAt = utcNow;

        recipientAccount.CurrentBalance += amount;
        recipientAccount.UpdatedAt = utcNow;

        transaction.LedgerEntries.Add(debitEntry);
        transaction.LedgerEntries.Add(creditEntry);

        _dbContext.Transactions.Add(transaction);

        return Task.CompletedTask;
    }
}

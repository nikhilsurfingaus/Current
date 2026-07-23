using Current.Api.Common;
using Current.Api.Common.Enums;
using Current.Api.Data;
using Current.Api.DTOs.Transactions;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Current.Api.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Services;

public class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly INotificationService _notificationService;

    public TransactionService(
        ApplicationDbContext dbContext,
        INotificationService notificationService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
    }

    public async Task<TransactionResponse> TransferFundsAsync(TransferRequest request, Guid currentUserId)
    {
        if (request.FromAccountId == request.ToAccountId)
        {
            throw new InvalidOperationException("Cannot transfer to the same account.");
        }

        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("Amount must be greater than zero.");
        }

        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var fromAccount = await _dbContext.Accounts
                .FirstOrDefaultAsync(account =>
                    account.Id == request.FromAccountId && account.UserId == currentUserId);

            if (fromAccount is null)
            {
                throw new InvalidOperationException("Source account not found.");
            }

            var toAccount = await _dbContext.Accounts
                .FirstOrDefaultAsync(account =>
                    account.Id == request.ToAccountId && account.UserId == currentUserId);

            if (toAccount is null)
            {
                throw new InvalidOperationException("Destination account not found.");
            }

            if (fromAccount.CurrentBalance < request.Amount)
            {
                throw new InvalidOperationException("Insufficient funds.");
            }

            var utcNow = DateTime.UtcNow;
            var transferAmount = request.Amount;

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                FromAccountId = request.FromAccountId,
                ToAccountId = request.ToAccountId,
                Amount = transferAmount,
                Description = request.Description.Trim(),
                Category = TransactionCategory.Transfer,
                Status = TransactionStatus.Completed,
                CreatedAt = utcNow
            };

            var debitEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = fromAccount.Id,
                EntryType = LedgerEntryType.Debit,
                Amount = transferAmount,
                CreatedAt = utcNow
            };

            var creditEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = toAccount.Id,
                EntryType = LedgerEntryType.Credit,
                Amount = transferAmount,
                CreatedAt = utcNow
            };

            fromAccount.CurrentBalance -= transferAmount;
            fromAccount.UpdatedAt = utcNow;

            toAccount.CurrentBalance += transferAmount;
            toAccount.UpdatedAt = utcNow;

            transaction.LedgerEntries.Add(debitEntry);
            transaction.LedgerEntries.Add(creditEntry);

            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            await _notificationService.TryCreateNotificationAsync(
                currentUserId,
                NotificationType.System,
                "Transfer completed",
                $"{NotificationFormatting.FormatAmount(transferAmount, fromAccount.Currency)} moved from {fromAccount.Name} to {toAccount.Name}.");

            return transaction.ToResponse();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IReadOnlyList<TransactionResponse>> GetAllTransactionsAsync(Guid currentUserId)
    {
        var userAccountIds = await _dbContext.Accounts
            .AsNoTracking()
            .Where(account => account.UserId == currentUserId)
            .Select(account => account.Id)
            .ToListAsync();

        var transactions = await _dbContext.Transactions
            .AsNoTracking()
            .Include(transaction => transaction.LedgerEntries)
            .Where(transaction =>
                userAccountIds.Contains(transaction.FromAccountId) ||
                userAccountIds.Contains(transaction.ToAccountId))
            .OrderByDescending(transaction => transaction.CreatedAt)
            .ToListAsync();

        return transactions.Select(transaction => transaction.ToResponse()).ToList();
    }

    public async Task<TransactionResponse?> GetTransactionByIdAsync(Guid transactionId, Guid currentUserId)
    {
        var userAccountIds = await _dbContext.Accounts
            .AsNoTracking()
            .Where(account => account.UserId == currentUserId)
            .Select(account => account.Id)
            .ToListAsync();

        var transaction = await _dbContext.Transactions
            .AsNoTracking()
            .Include(transaction => transaction.LedgerEntries)
            .FirstOrDefaultAsync(transaction =>
                transaction.Id == transactionId &&
                (userAccountIds.Contains(transaction.FromAccountId) ||
                 userAccountIds.Contains(transaction.ToAccountId)));

        return transaction?.ToResponse();
    }
}

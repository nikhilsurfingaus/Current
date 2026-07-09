using Current.Api.Common.Enums;
using Current.Api.Data;
using Current.Api.DTOs.Payments;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Current.Api.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _dbContext;

    public PaymentService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentReceiptResponse> SendPaymentAsync(SendPaymentRequest request, Guid currentUserId)
    {
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("Amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.RecipientEmail))
        {
            throw new InvalidOperationException("Recipient email is required.");
        }

        var recipientEmail = request.RecipientEmail.Trim().ToLowerInvariant();
        var paymentReference = string.IsNullOrWhiteSpace(request.Reference)
            ? null
            : request.Reference.Trim();

        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var senderAccount = await _dbContext.Accounts
                .FirstOrDefaultAsync(account =>
                    account.Id == request.FromAccountId && account.UserId == currentUserId);

            if (senderAccount is null)
            {
                throw new InvalidOperationException("Source account not found.");
            }

            var recipientUser = await _dbContext.Users
                .FirstOrDefaultAsync(user => user.Email.ToLower() == recipientEmail);

            if (recipientUser is null)
            {
                throw new InvalidOperationException("Recipient not found.");
            }

            if (recipientUser.Id == currentUserId)
            {
                throw new InvalidOperationException("Use account transfer for your own accounts.");
            }

            var recipientGoalAccountIds = await _dbContext.Goals
                .AsNoTracking()
                .Where(goal => goal.UserId == recipientUser.Id)
                .Select(goal => goal.GoalAccountId)
                .ToListAsync();

            var recipientAccount = await _dbContext.Accounts
                .Where(account =>
                    account.UserId == recipientUser.Id &&
                    !recipientGoalAccountIds.Contains(account.Id))
                .OrderBy(account => account.AccountType == AccountType.Everyday ? 0 : 1)
                .ThenBy(account => account.CreatedAt)
                .FirstOrDefaultAsync();

            if (recipientAccount is null)
            {
                throw new InvalidOperationException("Recipient account not found.");
            }

            if (!string.Equals(senderAccount.Currency, recipientAccount.Currency, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Currency not supported for this payment.");
            }

            if (senderAccount.CurrentBalance < request.Amount)
            {
                throw new InvalidOperationException("Insufficient funds.");
            }

            var utcNow = DateTime.UtcNow;
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                FromAccountId = senderAccount.Id,
                ToAccountId = recipientAccount.Id,
                Amount = request.Amount,
                Description = $"Payment to {recipientUser.FirstName} {recipientUser.LastName}".Trim(),
                Category = TransactionCategory.Transfer,
                Reference = paymentReference,
                Status = TransactionStatus.Completed,
                CreatedAt = utcNow
            };

            var debitEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = senderAccount.Id,
                EntryType = LedgerEntryType.Debit,
                Amount = request.Amount,
                CreatedAt = utcNow
            };

            var creditEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = recipientAccount.Id,
                EntryType = LedgerEntryType.Credit,
                Amount = request.Amount,
                CreatedAt = utcNow
            };

            senderAccount.CurrentBalance -= request.Amount;
            senderAccount.UpdatedAt = utcNow;

            recipientAccount.CurrentBalance += request.Amount;
            recipientAccount.UpdatedAt = utcNow;

            transaction.LedgerEntries.Add(debitEntry);
            transaction.LedgerEntries.Add(creditEntry);

            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return transaction.ToReceiptResponse(recipientAccount, recipientUser);
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }
}

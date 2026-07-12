using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Current.Api.Common.Enums;
using Current.Api.Common.Exceptions;
using Current.Api.Data;
using Current.Api.DTOs.Payments;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Current.Api.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Services;

public class PaymentService : IPaymentService
{
    private const string IdempotencyPendingMarker = "__PENDING__";
    private readonly ApplicationDbContext _dbContext;
    private static readonly TimeSpan IdempotencyKeyLifetime = TimeSpan.FromHours(24);
    private static readonly JsonSerializerOptions ReceiptJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public PaymentService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentReceiptResponse> SendPaymentAsync(
        SendPaymentRequest request,
        Guid currentUserId,
        string idempotencyKey)
    {
        ValidateRequest(request, idempotencyKey);

        var normalizedIdempotencyKey = idempotencyKey.Trim();
        var requestHash = BuildRequestHash(request);

        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var replayedReceipt = await TryReplayIdempotentPaymentAsync(
                currentUserId,
                normalizedIdempotencyKey,
                requestHash);

            if (replayedReceipt is not null)
            {
                await dbTransaction.CommitAsync();
                return replayedReceipt;
            }

            var utcNow = DateTime.UtcNow;
            var idempotencyRecord = new IdempotencyKey
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                Key = normalizedIdempotencyKey,
                RequestHash = requestHash,
                ResponseJson = IdempotencyPendingMarker,
                CreatedAt = utcNow,
                ExpiresAt = utcNow.Add(IdempotencyKeyLifetime)
            };

            _dbContext.IdempotencyKeys.Add(idempotencyRecord);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                await dbTransaction.RollbackAsync();

                var concurrentReplayReceipt = await TryReplayIdempotentPaymentAsync(
                    currentUserId,
                    normalizedIdempotencyKey,
                    requestHash);

                if (concurrentReplayReceipt is not null)
                {
                    return concurrentReplayReceipt;
                }

                throw new PaymentException(
                    PaymentErrorCode.DuplicatePayment,
                    "A payment with this idempotency key is already in progress.");
            }

            var receipt = await ExecutePaymentAsync(request, currentUserId);
            idempotencyRecord.ResponseJson = JsonSerializer.Serialize(receipt, ReceiptJsonOptions);
            await _dbContext.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return receipt;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    private static void ValidateRequest(SendPaymentRequest request, string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new PaymentException(
                PaymentErrorCode.IdempotencyKeyRequired,
                "Idempotency key is required.");
        }

        if (request.Amount <= 0)
        {
            throw new PaymentException(
                PaymentErrorCode.InvalidAmount,
                "Amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.RecipientEmail))
        {
            throw new PaymentException(
                PaymentErrorCode.RecipientEmailRequired,
                "Recipient email is required.");
        }
    }

    private async Task<PaymentReceiptResponse?> TryReplayIdempotentPaymentAsync(
        Guid currentUserId,
        string idempotencyKey,
        string requestHash)
    {
        var existingIdempotencyRecord = await _dbContext.IdempotencyKeys
            .FirstOrDefaultAsync(record =>
                record.UserId == currentUserId &&
                record.Key == idempotencyKey);

        if (existingIdempotencyRecord is null)
        {
            return null;
        }

        if (existingIdempotencyRecord.ExpiresAt <= DateTime.UtcNow)
        {
            _dbContext.IdempotencyKeys.Remove(existingIdempotencyRecord);
            await _dbContext.SaveChangesAsync();
            return null;
        }

        if (!string.Equals(existingIdempotencyRecord.RequestHash, requestHash, StringComparison.Ordinal))
        {
            throw new PaymentException(
                PaymentErrorCode.DuplicatePayment,
                "This idempotency key was already used with a different payment request.");
        }

        if (existingIdempotencyRecord.ResponseJson == IdempotencyPendingMarker)
        {
            throw new PaymentException(
                PaymentErrorCode.DuplicatePayment,
                "A payment with this idempotency key is already in progress.");
        }

        return DeserializeReceipt(existingIdempotencyRecord.ResponseJson);
    }

    private async Task<PaymentReceiptResponse> ExecutePaymentAsync(
        SendPaymentRequest request,
        Guid currentUserId)
    {
        var recipientEmail = request.RecipientEmail.Trim().ToLowerInvariant();
        var paymentReference = string.IsNullOrWhiteSpace(request.Reference)
            ? null
            : request.Reference.Trim();

        var senderAccount = await _dbContext.Accounts
            .FirstOrDefaultAsync(account =>
                account.Id == request.FromAccountId && account.UserId == currentUserId);

        if (senderAccount is null)
        {
            throw new PaymentException(
                PaymentErrorCode.SourceAccountNotFound,
                "Source account not found.");
        }

        var recipientUser = await _dbContext.Users
            .FirstOrDefaultAsync(user => user.Email.ToLower() == recipientEmail);

        if (recipientUser is null)
        {
            throw new PaymentException(
                PaymentErrorCode.RecipientNotFound,
                "Recipient not found.");
        }

        if (recipientUser.Id == currentUserId)
        {
            throw new PaymentException(
                PaymentErrorCode.SelfPaymentNotAllowed,
                "Use account transfer for your own accounts.");
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
            throw new PaymentException(
                PaymentErrorCode.RecipientAccountNotFound,
                "Recipient account not found.");
        }

        if (!string.Equals(senderAccount.Currency, recipientAccount.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new PaymentException(
                PaymentErrorCode.CurrencyNotSupported,
                "Currency not supported for this payment.");
        }

        if (senderAccount.CurrentBalance < request.Amount)
        {
            throw new PaymentException(
                PaymentErrorCode.InsufficientFunds,
                "Insufficient funds.");
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

        return transaction.ToReceiptResponse(recipientAccount, recipientUser);
    }

    private static string BuildRequestHash(SendPaymentRequest request)
    {
        var recipientEmail = request.RecipientEmail.Trim().ToLowerInvariant();
        var paymentReference = string.IsNullOrWhiteSpace(request.Reference)
            ? string.Empty
            : request.Reference.Trim();

        var requestFingerprint = string.Join(
            '|',
            request.FromAccountId,
            recipientEmail,
            request.Amount.ToString("0.00"),
            paymentReference);

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(requestFingerprint));
        return Convert.ToHexString(hashBytes);
    }

    private static PaymentReceiptResponse DeserializeReceipt(string responseJson)
    {
        var receipt = JsonSerializer.Deserialize<PaymentReceiptResponse>(responseJson, ReceiptJsonOptions);

        if (receipt is null)
        {
            throw new PaymentException(
                PaymentErrorCode.DuplicatePayment,
                "Stored payment receipt could not be replayed.");
        }

        return receipt;
    }
}

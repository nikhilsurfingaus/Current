using Current.Api.Common;
using Current.Api.Common.Constants;
using Current.Api.Common.Enums;
using Current.Api.Data;
using Current.Api.DTOs.Branches;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Services;

public class BranchService : IBranchService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDisbursementService _disbursementService;

    public BranchService(
        ApplicationDbContext dbContext,
        IDisbursementService disbursementService)
    {
        _dbContext = dbContext;
        _disbursementService = disbursementService;
    }

    public async Task<BranchTreasuryResponse> GetTreasuryAsync()
    {
        var branch = await _disbursementService.GetDefaultBranchAsync();

        return new BranchTreasuryResponse
        {
            BranchId = branch.Id,
            BranchName = branch.Name,
            BranchCode = branch.Code,
            TreasuryBalance = branch.TreasuryAccount.CurrentBalance,
            Currency = branch.TreasuryAccount.Currency
        };
    }

    public async Task<BranchDisbursementResponse> CreateDisbursementAsync(
        CreateBranchDisbursementRequest request)
    {
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("Amount must be greater than zero.");
        }

        var hasRecipientEmail = !string.IsNullOrWhiteSpace(request.RecipientEmail);
        var hasRecipientAccountId = request.RecipientAccountId.HasValue
            && request.RecipientAccountId.Value != Guid.Empty;
        var hasBsbRecipient = !string.IsNullOrWhiteSpace(request.RecipientBsb) ||
                                !string.IsNullOrWhiteSpace(request.RecipientAccountNumber);
        var recipientMethodCount = (hasRecipientEmail ? 1 : 0)
            + (hasRecipientAccountId ? 1 : 0)
            + (hasBsbRecipient ? 1 : 0);

        if (recipientMethodCount != 1)
        {
            throw new InvalidOperationException(
                "Provide exactly one of recipientEmail, recipientAccountId, or BSB and account number.");
        }

        if (hasBsbRecipient &&
            (!BankAccountNormalizer.TryNormalizeBsb(request.RecipientBsb, out _) ||
             !BankAccountNormalizer.TryNormalizeAccountNumber(request.RecipientAccountNumber, out _)))
        {
            throw new InvalidOperationException("Enter a valid BSB and account number.");
        }

        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var branch = await _disbursementService.GetDefaultBranchAsync();
            var treasuryAccount = await _dbContext.Accounts
                .FirstAsync(account => account.Id == branch.TreasuryAccountId);

            var recipientAccount = hasRecipientAccountId
                ? await ResolveRecipientAccountByIdAsync(request.RecipientAccountId!.Value)
                : hasRecipientEmail
                    ? await ResolveRecipientAccountByEmailAsync(request.RecipientEmail!)
                    : await ResolveRecipientAccountByBsbAsync(
                        request.RecipientBsb!,
                        request.RecipientAccountNumber!);

            var recipientUser = await _dbContext.Users
                .AsNoTracking()
                .FirstAsync(user => user.Id == recipientAccount.UserId);

            var description = string.IsNullOrWhiteSpace(request.Description)
                ? BranchConstants.BranchTopUpDescription
                : request.Description.Trim();

            await _disbursementService.ApplyDisbursementAsync(
                treasuryAccount,
                recipientAccount,
                request.Amount,
                description,
                TransactionCategory.Income);

            await _dbContext.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            var disbursementTransaction = await _dbContext.Transactions
                .AsNoTracking()
                .Where(transaction =>
                    transaction.FromAccountId == treasuryAccount.Id &&
                    transaction.ToAccountId == recipientAccount.Id)
                .OrderByDescending(transaction => transaction.CreatedAt)
                .FirstAsync();

            return new BranchDisbursementResponse
            {
                TransactionId = disbursementTransaction.Id,
                RecipientAccountId = recipientAccount.Id,
                RecipientEmail = recipientUser.Email,
                RecipientName = $"{recipientUser.FirstName} {recipientUser.LastName}".Trim(),
                Amount = request.Amount,
                Currency = recipientAccount.Currency,
                Description = description,
                TreasuryBalanceAfter = treasuryAccount.CurrentBalance,
                CreatedAt = disbursementTransaction.CreatedAt
            };
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    private async Task<Account> ResolveRecipientAccountByEmailAsync(string recipientEmail)
    {
        var normalizedEmail = recipientEmail.Trim().ToLowerInvariant();

        var recipientUser = await _dbContext.Users
            .FirstOrDefaultAsync(user => user.Email.ToLower() == normalizedEmail);

        if (recipientUser is null)
        {
            throw new InvalidOperationException("Recipient user not found.");
        }

        if (recipientUser.Id == BranchConstants.SystemUserId)
        {
            throw new InvalidOperationException("Cannot disburse to the branch system user.");
        }

        return await ResolveDefaultRecipientAccountAsync(recipientUser.Id);
    }

    private async Task<Account> ResolveRecipientAccountByIdAsync(Guid recipientAccountId)
    {
        var recipientAccount = await _dbContext.Accounts
            .FirstOrDefaultAsync(account => account.Id == recipientAccountId);

        if (recipientAccount is null)
        {
            throw new InvalidOperationException("Recipient account not found.");
        }

        if (recipientAccount.AccountType == AccountType.Branch)
        {
            throw new InvalidOperationException("Cannot disburse to a branch treasury account.");
        }

        if (recipientAccount.UserId == BranchConstants.SystemUserId)
        {
            throw new InvalidOperationException("Cannot disburse to the branch system user.");
        }

        var isGoalAccount = await _dbContext.Goals
            .AsNoTracking()
            .AnyAsync(goal => goal.GoalAccountId == recipientAccount.Id);

        if (isGoalAccount)
        {
            throw new InvalidOperationException("Cannot disburse to a goal account.");
        }

        return recipientAccount;
    }

    private async Task<Account> ResolveRecipientAccountByBsbAsync(
        string recipientBsb,
        string recipientAccountNumber)
    {
        var normalizedBsb = BankAccountNormalizer.NormalizeBsb(recipientBsb);
        var normalizedAccountNumber = BankAccountNormalizer.NormalizeAccountNumber(recipientAccountNumber);

        var recipientAccount = await _dbContext.Accounts
            .FirstOrDefaultAsync(account =>
                account.Bsb == normalizedBsb &&
                account.AccountNumber == normalizedAccountNumber);

        if (recipientAccount is null)
        {
            throw new InvalidOperationException("Recipient account not found.");
        }

        if (recipientAccount.AccountType == AccountType.Branch)
        {
            throw new InvalidOperationException("Cannot disburse to a branch treasury account.");
        }

        if (recipientAccount.UserId == BranchConstants.SystemUserId)
        {
            throw new InvalidOperationException("Cannot disburse to the branch system user.");
        }

        var isGoalAccount = await _dbContext.Goals
            .AsNoTracking()
            .AnyAsync(goal => goal.GoalAccountId == recipientAccount.Id);

        if (isGoalAccount)
        {
            throw new InvalidOperationException("Cannot disburse to a goal account.");
        }

        return recipientAccount;
    }

    private async Task<Account> ResolveDefaultRecipientAccountAsync(Guid recipientUserId)
    {
        var goalAccountIds = await _dbContext.Goals
            .AsNoTracking()
            .Where(goal => goal.UserId == recipientUserId)
            .Select(goal => goal.GoalAccountId)
            .ToListAsync();

        var recipientAccount = await _dbContext.Accounts
            .Where(account =>
                account.UserId == recipientUserId &&
                account.AccountType != AccountType.Branch &&
                !goalAccountIds.Contains(account.Id))
            .OrderBy(account => account.AccountType == AccountType.Everyday ? 0 : 1)
            .ThenBy(account => account.CreatedAt)
            .FirstOrDefaultAsync();

        if (recipientAccount is null)
        {
            throw new InvalidOperationException("Recipient has no eligible account.");
        }

        return recipientAccount;
    }
}

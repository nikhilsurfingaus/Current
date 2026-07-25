using Current.Api.Common;
using Current.Api.Common.Constants;
using Current.Api.Common.Enums;
using Current.Api.Data;
using Current.Api.DTOs.Goals;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Current.Api.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Services;

public class GoalService : IGoalService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IBankAccountNumberService _bankAccountNumberService;

    public GoalService(
        ApplicationDbContext dbContext,
        INotificationService notificationService,
        IBankAccountNumberService bankAccountNumberService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _bankAccountNumberService = bankAccountNumberService;
    }

    public async Task<IReadOnlyList<GoalResponse>> GetAllGoalsAsync(Guid currentUserId)
    {
        var goals = await _dbContext.Goals
            .AsNoTracking()
            .Where(goal => goal.UserId == currentUserId)
            .OrderBy(goal => goal.Name)
            .ToListAsync();

        return goals.Select(goal => goal.ToResponse()).ToList();
    }

    public async Task<GoalResponse?> GetGoalByIdAsync(Guid goalId, Guid currentUserId)
    {
        var goal = await FindOwnedGoalAsync(goalId, currentUserId, asNoTracking: true);
        return goal?.ToResponse();
    }

    public async Task<GoalResponse> CreateGoalAsync(CreateGoalRequest request, Guid currentUserId)
    {
        if (request.TargetAmount <= 0)
        {
            throw new InvalidOperationException("Target amount must be greater than zero.");
        }

        var goalName = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(goalName))
        {
            throw new InvalidOperationException("Goal name is required.");
        }

        var goalCurrency = request.Currency.Trim().ToUpperInvariant();
        if (goalCurrency.Length != 3)
        {
            throw new InvalidOperationException("Currency must be a 3-letter code.");
        }

        var sourceAccount = await _dbContext.Accounts
            .FirstOrDefaultAsync(account =>
                account.Id == request.SourceAccountId && account.UserId == currentUserId);

        if (sourceAccount is null)
        {
            throw new InvalidOperationException("Source account not found.");
        }

        if (!string.Equals(sourceAccount.Currency, goalCurrency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Goal currency must match the source account currency.");
        }

        var utcNow = DateTime.UtcNow;
        var goalAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Name = goalName,
            AccountType = AccountType.Savings,
            CurrentBalance = 0,
            Currency = goalCurrency,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        await _bankAccountNumberService.AssignBankDetailsAsync(goalAccount);

        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            SourceAccountId = sourceAccount.Id,
            GoalAccountId = goalAccount.Id,
            Name = goalName,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            TargetAmount = request.TargetAmount,
            CurrentAmount = 0,
            Currency = goalCurrency,
            TargetDate = request.TargetDate,
            Status = GoalStatus.Active,
            IconKey = GoalIconKeys.Normalize(request.IconKey),
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        _dbContext.Accounts.Add(goalAccount);
        _dbContext.Goals.Add(goal);
        await _dbContext.SaveChangesAsync();

        return goal.ToResponse();
    }

    public async Task<GoalResponse?> UpdateGoalAsync(Guid goalId, UpdateGoalRequest request, Guid currentUserId)
    {
        var goal = await FindOwnedGoalAsync(goalId, currentUserId);

        if (goal is null)
        {
            return null;
        }

        if (goal.Status == GoalStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled goals cannot be updated.");
        }

        var goalName = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(goalName))
        {
            throw new InvalidOperationException("Goal name is required.");
        }

        if (request.TargetAmount <= 0)
        {
            throw new InvalidOperationException("Target amount must be greater than zero.");
        }

        goal.Name = goalName;
        goal.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        goal.TargetAmount = request.TargetAmount;
        goal.TargetDate = request.TargetDate;
        goal.Status = request.Status;
        if (request.IconKey is not null)
        {
            goal.IconKey = GoalIconKeys.Normalize(request.IconKey);
        }

        goal.UpdatedAt = DateTime.UtcNow;

        var goalAccount = await _dbContext.Accounts
            .FirstOrDefaultAsync(account => account.Id == goal.GoalAccountId && account.UserId == currentUserId);

        if (goalAccount is not null)
        {
            goalAccount.Name = goalName;
            goalAccount.UpdatedAt = goal.UpdatedAt;
        }

        await _dbContext.SaveChangesAsync();

        return goal.ToResponse();
    }

    public async Task<GoalResponse?> CancelGoalAsync(Guid goalId, Guid currentUserId)
    {
        var goal = await FindOwnedGoalAsync(goalId, currentUserId);

        if (goal is null)
        {
            return null;
        }

        if (goal.Status == GoalStatus.Cancelled)
        {
            return goal.ToResponse();
        }

        goal.Status = GoalStatus.Cancelled;
        goal.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return goal.ToResponse();
    }

    public async Task<GoalResponse> ContributeToGoalAsync(
        Guid goalId,
        ContributeGoalRequest request,
        Guid currentUserId)
    {
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("Amount must be greater than zero.");
        }

        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var goal = await FindOwnedGoalAsync(goalId, currentUserId);

            if (goal is null)
            {
                throw new InvalidOperationException("Goal not found.");
            }

            EnsureGoalAcceptsContributions(goal);

            var sourceAccount = await _dbContext.Accounts
                .FirstOrDefaultAsync(account =>
                    account.Id == goal.SourceAccountId && account.UserId == currentUserId);

            var goalAccount = await _dbContext.Accounts
                .FirstOrDefaultAsync(account =>
                    account.Id == goal.GoalAccountId && account.UserId == currentUserId);

            if (sourceAccount is null || goalAccount is null)
            {
                throw new InvalidOperationException("Goal accounts are not available.");
            }

            if (sourceAccount.CurrentBalance < request.Amount)
            {
                throw new InvalidOperationException("Insufficient funds in source account.");
            }

            var contributionNote = string.IsNullOrWhiteSpace(request.Note)
                ? $"Contribution to {goal.Name}"
                : request.Note.Trim();

            var transaction = await ApplyTransferAsync(
                sourceAccount,
                goalAccount,
                request.Amount,
                contributionNote);

            var contribution = new GoalContribution
            {
                Id = Guid.NewGuid(),
                GoalId = goal.Id,
                TransactionId = transaction.Id,
                Amount = request.Amount,
                ContributionType = ContributionType.Deposit,
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                CreatedAt = transaction.CreatedAt
            };

            _dbContext.GoalContributions.Add(contribution);

            goal.CurrentAmount = goalAccount.CurrentBalance;
            goal.UpdatedAt = transaction.CreatedAt;

            var wasGoalCompleted = goal.Status == GoalStatus.Completed;

            if (goal.CurrentAmount >= goal.TargetAmount)
            {
                goal.Status = GoalStatus.Completed;
            }

            var goalJustCompleted = !wasGoalCompleted && goal.Status == GoalStatus.Completed;
            var goalName = goal.Name;
            var contributionAmount = request.Amount;
            var goalCurrency = goal.Currency;

            await _dbContext.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            var amountLabel = NotificationFormatting.FormatAmount(contributionAmount, goalCurrency);

            await _notificationService.TryCreateNotificationAsync(
                currentUserId,
                NotificationType.GoalContribution,
                "Goal contribution",
                $"You added {amountLabel} to {goalName}.");

            if (goalJustCompleted)
            {
                await _notificationService.TryCreateNotificationAsync(
                    currentUserId,
                    NotificationType.GoalCompleted,
                    "Goal completed",
                    $"{goalName} is fully funded.");
            }

            return goal.ToResponse();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<GoalResponse> WithdrawFromGoalAsync(
        Guid goalId,
        WithdrawGoalRequest request,
        Guid currentUserId)
    {
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("Amount must be greater than zero.");
        }

        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var goal = await FindOwnedGoalAsync(goalId, currentUserId);

            if (goal is null)
            {
                throw new InvalidOperationException("Goal not found.");
            }

            if (goal.Status == GoalStatus.Cancelled)
            {
                throw new InvalidOperationException("Cancelled goals cannot accept withdrawals.");
            }

            var goalAccount = await _dbContext.Accounts
                .FirstOrDefaultAsync(account =>
                    account.Id == goal.GoalAccountId && account.UserId == currentUserId);

            var destinationAccount = await _dbContext.Accounts
                .FirstOrDefaultAsync(account =>
                    account.Id == request.DestinationAccountId && account.UserId == currentUserId);

            if (goalAccount is null || destinationAccount is null)
            {
                throw new InvalidOperationException("Goal accounts are not available.");
            }

            if (goalAccount.Id == destinationAccount.Id)
            {
                throw new InvalidOperationException("Destination account must be different from the goal account.");
            }

            if (!string.Equals(goalAccount.Currency, destinationAccount.Currency, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Destination account currency must match the goal currency.");
            }

            if (goalAccount.CurrentBalance < request.Amount)
            {
                throw new InvalidOperationException("Insufficient funds in goal account.");
            }

            var withdrawalNote = string.IsNullOrWhiteSpace(request.Note)
                ? $"Withdrawal from {goal.Name}"
                : request.Note.Trim();

            var transaction = await ApplyTransferAsync(
                goalAccount,
                destinationAccount,
                request.Amount,
                withdrawalNote);

            var contribution = new GoalContribution
            {
                Id = Guid.NewGuid(),
                GoalId = goal.Id,
                TransactionId = transaction.Id,
                Amount = request.Amount,
                ContributionType = ContributionType.Withdrawal,
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                CreatedAt = transaction.CreatedAt
            };

            _dbContext.GoalContributions.Add(contribution);

            goal.CurrentAmount = goalAccount.CurrentBalance;
            goal.UpdatedAt = transaction.CreatedAt;

            if (goal.Status == GoalStatus.Completed && goal.CurrentAmount < goal.TargetAmount)
            {
                goal.Status = GoalStatus.Active;
            }

            await _dbContext.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return goal.ToResponse();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IReadOnlyList<GoalContributionResponse>> GetContributionHistoryAsync(
        Guid goalId,
        Guid currentUserId)
    {
        var goalExists = await _dbContext.Goals
            .AsNoTracking()
            .AnyAsync(goal => goal.Id == goalId && goal.UserId == currentUserId);

        if (!goalExists)
        {
            return Array.Empty<GoalContributionResponse>();
        }

        var contributions = await _dbContext.GoalContributions
            .AsNoTracking()
            .Where(contribution => contribution.GoalId == goalId)
            .OrderByDescending(contribution => contribution.CreatedAt)
            .ToListAsync();

        return contributions.Select(contribution => contribution.ToResponse()).ToList();
    }

    private async Task<Goal?> FindOwnedGoalAsync(
        Guid goalId,
        Guid currentUserId,
        bool asNoTracking = false)
    {
        var query = _dbContext.Goals.AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(goal =>
            goal.Id == goalId && goal.UserId == currentUserId);
    }

    private static void EnsureGoalAcceptsContributions(Goal goal)
    {
        if (goal.Status == GoalStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled goals cannot accept contributions.");
        }

        if (goal.Status == GoalStatus.Completed)
        {
            throw new InvalidOperationException("Completed goals cannot accept contributions.");
        }
    }

    private async Task<Transaction> ApplyTransferAsync(
        Account fromAccount,
        Account toAccount,
        decimal amount,
        string description)
    {
        if (fromAccount.Id == toAccount.Id)
        {
            throw new InvalidOperationException("Cannot transfer to the same account.");
        }

        if (!string.Equals(fromAccount.Currency, toAccount.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Accounts must use the same currency.");
        }

        var utcNow = DateTime.UtcNow;

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            FromAccountId = fromAccount.Id,
            ToAccountId = toAccount.Id,
            Amount = amount,
            Description = description,
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
            Amount = amount,
            CreatedAt = utcNow
        };

        var creditEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            TransactionId = transaction.Id,
            AccountId = toAccount.Id,
            EntryType = LedgerEntryType.Credit,
            Amount = amount,
            CreatedAt = utcNow
        };

        fromAccount.CurrentBalance -= amount;
        fromAccount.UpdatedAt = utcNow;

        toAccount.CurrentBalance += amount;
        toAccount.UpdatedAt = utcNow;

        transaction.LedgerEntries.Add(debitEntry);
        transaction.LedgerEntries.Add(creditEntry);

        _dbContext.Transactions.Add(transaction);
        await _dbContext.SaveChangesAsync();

        return transaction;
    }
}

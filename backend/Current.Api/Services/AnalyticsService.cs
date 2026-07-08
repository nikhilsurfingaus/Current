using Current.Api.Common.Enums;
using Current.Api.Data;
using Current.Api.DTOs.Analytics;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Current.Api.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ApplicationDbContext _dbContext;
    private const int CashFlowMonthWindow = 6;
    private const int NetWorthHistoryDayWindow = 30;

    public AnalyticsService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AnalyticsOverviewResponse> GetOverviewAsync(Guid currentUserId)
    {
        var userAccounts = await GetUserAccountsAsync(currentUserId);
        var userAccountIds = userAccounts.Select(account => account.Id).ToList();
        var goals = await GetUserGoalsAsync(currentUserId);

        var totalBalance = userAccounts.Sum(account => account.CurrentBalance);
        var activeGoals = goals.Where(goal => goal.Status == GoalStatus.Active).ToList();
        var completedGoals = goals.Where(goal => goal.Status == GoalStatus.Completed).ToList();
        var totalGoalSavings = goals
            .Where(goal => goal.Status != GoalStatus.Cancelled)
            .Sum(goal => goal.CurrentAmount);

        var (monthStartUtc, monthEndUtc) = GetCurrentMonthUtcBounds();
        var monthTransactions = await GetUserTransactionsQuery(userAccountIds)
            .Where(transaction => transaction.CreatedAt >= monthStartUtc && transaction.CreatedAt < monthEndUtc)
            .ToListAsync();

        var monthlyIncome = monthTransactions
            .Where(transaction => transaction.Category == TransactionCategory.Income)
            .Sum(transaction => transaction.Amount);
        var monthlyExpenses = monthTransactions
            .Where(transaction => IsExpenseCategory(transaction.Category))
            .Sum(transaction => transaction.Amount);
        var netCashFlow = monthlyIncome - monthlyExpenses;
        var savingsRatePercent = monthlyIncome > 0
            ? Math.Round(netCashFlow / monthlyIncome * 100, 2)
            : 0;

        return new AnalyticsOverviewResponse
        {
            TotalBalance = totalBalance,
            MonthlyIncome = monthlyIncome,
            MonthlyExpenses = monthlyExpenses,
            NetCashFlow = netCashFlow,
            SavingsRatePercent = savingsRatePercent,
            ActiveGoalsCount = activeGoals.Count,
            CompletedGoalsCount = completedGoals.Count,
            TotalGoalSavings = totalGoalSavings,
        };
    }

    public async Task<CashFlowResponse> GetCashFlowAsync(Guid currentUserId)
    {
        var userAccounts = await GetUserAccountsAsync(currentUserId);
        var userAccountIds = userAccounts.Select(account => account.Id).ToList();
        var todayUtc = DateTime.UtcNow.Date;
        var firstMonthStartUtc = new DateTime(todayUtc.Year, todayUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMonths(-(CashFlowMonthWindow - 1));
        var afterLastMonthUtc = new DateTime(todayUtc.Year, todayUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMonths(1);

        var transactions = await GetUserTransactionsQuery(userAccountIds)
            .Where(transaction =>
                transaction.CreatedAt >= firstMonthStartUtc &&
                transaction.CreatedAt < afterLastMonthUtc)
            .ToListAsync();

        var monthBuckets = Enumerable.Range(0, CashFlowMonthWindow)
            .Select(offset => firstMonthStartUtc.AddMonths(offset))
            .ToList();

        var monthPoints = monthBuckets.Select(bucketStart =>
        {
            var bucketEnd = bucketStart.AddMonths(1);
            var bucketTransactions = transactions
                .Where(transaction => transaction.CreatedAt >= bucketStart && transaction.CreatedAt < bucketEnd)
                .ToList();

            var income = bucketTransactions
                .Where(transaction => transaction.Category == TransactionCategory.Income)
                .Sum(transaction => transaction.Amount);
            var expenses = bucketTransactions
                .Where(transaction => IsExpenseCategory(transaction.Category))
                .Sum(transaction => transaction.Amount);

            return new CashFlowMonthPoint
            {
                Month = bucketStart.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                Income = income,
                Expenses = expenses,
                Net = income - expenses,
            };
        }).ToList();

        return new CashFlowResponse
        {
            Months = monthPoints,
        };
    }

    public async Task<NetWorthHistoryResponse> GetNetWorthHistoryAsync(Guid currentUserId)
    {
        var userAccounts = await GetUserAccountsAsync(currentUserId);
        var userAccountIds = userAccounts.Select(account => account.Id).ToList();
        var dayStartUtc = DateTime.UtcNow.Date.AddDays(-(NetWorthHistoryDayWindow - 1));
        var currentTotalBalance = userAccounts.Sum(account => account.CurrentBalance);

        var ledgerEntries = await _dbContext.LedgerEntries
            .AsNoTracking()
            .Where(ledgerEntry =>
                userAccountIds.Contains(ledgerEntry.AccountId) &&
                ledgerEntry.CreatedAt >= dayStartUtc)
            .ToListAsync();

        var points = Enumerable.Range(0, NetWorthHistoryDayWindow)
            .Select(offset =>
            {
                var currentDate = dayStartUtc.AddDays(offset).Date;
                var nextDate = currentDate.AddDays(1);
                var netChangeAfterDate = ledgerEntries
                    .Where(ledgerEntry => ledgerEntry.CreatedAt >= nextDate)
                    .Sum(ledgerEntry => ledgerEntry.EntryType == LedgerEntryType.Credit
                        ? ledgerEntry.Amount
                        : -ledgerEntry.Amount);

                return new NetWorthHistoryPoint
                {
                    Date = DateOnly.FromDateTime(currentDate),
                    Balance = currentTotalBalance - netChangeAfterDate,
                };
            })
            .ToList();

        return new NetWorthHistoryResponse
        {
            Points = points,
        };
    }

    public async Task<CategoryBreakdownResponse> GetCategoryBreakdownAsync(Guid currentUserId)
    {
        var userAccounts = await GetUserAccountsAsync(currentUserId);
        var userAccountIds = userAccounts.Select(account => account.Id).ToList();
        var (monthStartUtc, monthEndUtc) = GetCurrentMonthUtcBounds();

        var currentMonthTransactions = await GetUserTransactionsQuery(userAccountIds)
            .Where(transaction => transaction.CreatedAt >= monthStartUtc && transaction.CreatedAt < monthEndUtc)
            .Where(transaction => IsExpenseCategory(transaction.Category))
            .ToListAsync();

        var totalExpenses = currentMonthTransactions.Sum(transaction => transaction.Amount);
        var categoryItems = currentMonthTransactions
            .GroupBy(transaction => transaction.Category)
            .Select(group =>
            {
                var categoryAmount = group.Sum(transaction => transaction.Amount);
                var categoryPercent = totalExpenses > 0
                    ? Math.Round(categoryAmount / totalExpenses * 100, 2)
                    : 0;

                return new CategoryBreakdownItem
                {
                    Category = group.Key,
                    Amount = categoryAmount,
                    Percent = categoryPercent,
                };
            })
            .OrderByDescending(item => item.Amount)
            .ToList();

        return new CategoryBreakdownResponse
        {
            Categories = categoryItems,
        };
    }

    public async Task<GoalProgressResponse> GetGoalProgressAsync(Guid currentUserId)
    {
        var goals = await _dbContext.Goals
            .AsNoTracking()
            .Where(goal => goal.UserId == currentUserId && goal.Status != GoalStatus.Cancelled)
            .OrderBy(goal => goal.Name)
            .ToListAsync();

        var goalAnalyticsItems = goals.Select(goal =>
        {
            var completionPercent = goal.TargetAmount > 0
                ? Math.Min(100, Math.Round(goal.CurrentAmount / goal.TargetAmount * 100, 2))
                : 0;

            return new GoalAnalyticsItem
            {
                GoalId = goal.Id,
                Name = goal.Name,
                TargetAmount = goal.TargetAmount,
                CurrentAmount = goal.CurrentAmount,
                CompletionPercent = completionPercent,
                RemainingAmount = Math.Max(0, goal.TargetAmount - goal.CurrentAmount),
                ProjectedCompletionDate = null,
            };
        }).ToList();

        return new GoalProgressResponse
        {
            TotalSaved = goalAnalyticsItems.Sum(goal => goal.CurrentAmount),
            TotalRemaining = goalAnalyticsItems.Sum(goal => goal.RemainingAmount),
            Goals = goalAnalyticsItems,
        };
    }

    public async Task<MonthlySummaryResponse> GetMonthlySummaryAsync(Guid currentUserId)
    {
        var userAccounts = await GetUserAccountsAsync(currentUserId);
        var userAccountIds = userAccounts.Select(account => account.Id).ToList();
        var (monthStartUtc, monthEndUtc) = GetCurrentMonthUtcBounds();

        var monthTransactions = await GetUserTransactionsQuery(userAccountIds)
            .Where(transaction => transaction.CreatedAt >= monthStartUtc && transaction.CreatedAt < monthEndUtc)
            .ToListAsync();

        var income = monthTransactions
            .Where(transaction => transaction.Category == TransactionCategory.Income)
            .Sum(transaction => transaction.Amount);
        var expenses = monthTransactions
            .Where(transaction => IsExpenseCategory(transaction.Category))
            .Sum(transaction => transaction.Amount);
        var transfers = monthTransactions
            .Where(transaction => transaction.Category == TransactionCategory.Transfer)
            .Sum(transaction => transaction.Amount);
        var averageTransactionAmount = monthTransactions.Count > 0
            ? Math.Round(monthTransactions.Average(transaction => transaction.Amount), 2)
            : 0;
        var largestExpense = monthTransactions
            .Where(transaction => IsExpenseCategory(transaction.Category))
            .Select(transaction => transaction.Amount)
            .DefaultIfEmpty(0)
            .Max();
        var largestIncome = monthTransactions
            .Where(transaction => transaction.Category == TransactionCategory.Income)
            .Select(transaction => transaction.Amount)
            .DefaultIfEmpty(0)
            .Max();

        return new MonthlySummaryResponse
        {
            TransactionCount = monthTransactions.Count,
            Income = income,
            Expenses = expenses,
            Transfers = transfers,
            AverageTransactionAmount = averageTransactionAmount,
            LargestExpense = largestExpense,
            LargestIncome = largestIncome,
        };
    }

    private async Task<List<Account>> GetUserAccountsAsync(Guid currentUserId)
    {
        return await _dbContext.Accounts
            .AsNoTracking()
            .Where(account => account.UserId == currentUserId)
            .ToListAsync();
    }

    private async Task<List<Goal>> GetUserGoalsAsync(Guid currentUserId)
    {
        return await _dbContext.Goals
            .AsNoTracking()
            .Where(goal => goal.UserId == currentUserId)
            .ToListAsync();
    }

    private IQueryable<Transaction> GetUserTransactionsQuery(IReadOnlyList<Guid> userAccountIds)
    {
        return _dbContext.Transactions
            .AsNoTracking()
            .Where(transaction =>
                userAccountIds.Contains(transaction.FromAccountId) ||
                userAccountIds.Contains(transaction.ToAccountId));
    }

    private static (DateTime monthStartUtc, DateTime monthEndUtc) GetCurrentMonthUtcBounds()
    {
        var nowUtc = DateTime.UtcNow;
        var monthStartUtc = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEndUtc = monthStartUtc.AddMonths(1);
        return (monthStartUtc, monthEndUtc);
    }

    private static bool IsExpenseCategory(TransactionCategory category)
    {
        return category != TransactionCategory.Income && category != TransactionCategory.Transfer;
    }
}

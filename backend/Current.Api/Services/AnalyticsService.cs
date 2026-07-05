using Current.Api.Common.Enums;
using Current.Api.Data;
using Current.Api.DTOs.Analytics;
using Current.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ApplicationDbContext _dbContext;

    public AnalyticsService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AnalyticsOverviewResponse> GetOverviewAsync(Guid currentUserId)
    {
        var accounts = await _dbContext.Accounts
            .AsNoTracking()
            .Where(account => account.UserId == currentUserId)
            .ToListAsync();

        var goals = await _dbContext.Goals
            .AsNoTracking()
            .Where(goal => goal.UserId == currentUserId)
            .ToListAsync();

        var totalBalance = accounts.Sum(account => account.CurrentBalance);
        var activeGoals = goals.Where(goal => goal.Status == GoalStatus.Active).ToList();
        var completedGoals = goals.Where(goal => goal.Status == GoalStatus.Completed).ToList();
        var totalGoalSavings = goals
            .Where(goal => goal.Status != GoalStatus.Cancelled)
            .Sum(goal => goal.CurrentAmount);

        return new AnalyticsOverviewResponse
        {
            TotalBalance = totalBalance,
            MonthlyIncome = 0,
            MonthlyExpenses = 0,
            NetCashFlow = 0,
            SavingsRatePercent = 0,
            ActiveGoalsCount = activeGoals.Count,
            CompletedGoalsCount = completedGoals.Count,
            TotalGoalSavings = totalGoalSavings,
        };
    }

    public Task<CashFlowResponse> GetCashFlowAsync(Guid currentUserId)
    {
        return Task.FromResult(new CashFlowResponse());
    }

    public Task<NetWorthHistoryResponse> GetNetWorthHistoryAsync(Guid currentUserId)
    {
        return Task.FromResult(new NetWorthHistoryResponse());
    }

    public Task<CategoryBreakdownResponse> GetCategoryBreakdownAsync(Guid currentUserId)
    {
        return Task.FromResult(new CategoryBreakdownResponse());
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

    public Task<MonthlySummaryResponse> GetMonthlySummaryAsync(Guid currentUserId)
    {
        return Task.FromResult(new MonthlySummaryResponse());
    }
}

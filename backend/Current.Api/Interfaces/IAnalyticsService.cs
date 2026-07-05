using Current.Api.DTOs.Analytics;

namespace Current.Api.Interfaces;

public interface IAnalyticsService
{
    Task<AnalyticsOverviewResponse> GetOverviewAsync(Guid currentUserId);

    Task<CashFlowResponse> GetCashFlowAsync(Guid currentUserId);

    Task<NetWorthHistoryResponse> GetNetWorthHistoryAsync(Guid currentUserId);

    Task<CategoryBreakdownResponse> GetCategoryBreakdownAsync(Guid currentUserId);

    Task<GoalProgressResponse> GetGoalProgressAsync(Guid currentUserId);

    Task<MonthlySummaryResponse> GetMonthlySummaryAsync(Guid currentUserId);
}

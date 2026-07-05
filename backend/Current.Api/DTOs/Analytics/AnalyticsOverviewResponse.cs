namespace Current.Api.DTOs.Analytics;

public class AnalyticsOverviewResponse
{
    public decimal TotalBalance { get; set; }

    public decimal MonthlyIncome { get; set; }

    public decimal MonthlyExpenses { get; set; }

    public decimal NetCashFlow { get; set; }

    public decimal SavingsRatePercent { get; set; }

    public int ActiveGoalsCount { get; set; }

    public int CompletedGoalsCount { get; set; }

    public decimal TotalGoalSavings { get; set; }
}

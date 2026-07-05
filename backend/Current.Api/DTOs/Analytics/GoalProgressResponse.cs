namespace Current.Api.DTOs.Analytics;

public class GoalAnalyticsItem
{
    public Guid GoalId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal TargetAmount { get; set; }

    public decimal CurrentAmount { get; set; }

    public decimal CompletionPercent { get; set; }

    public decimal RemainingAmount { get; set; }

    public DateOnly? ProjectedCompletionDate { get; set; }
}

public class GoalProgressResponse
{
    public decimal TotalSaved { get; set; }

    public decimal TotalRemaining { get; set; }

    public IReadOnlyList<GoalAnalyticsItem> Goals { get; set; } = Array.Empty<GoalAnalyticsItem>();
}

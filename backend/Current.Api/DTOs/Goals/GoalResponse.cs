using Current.Api.Common.Enums;

namespace Current.Api.DTOs.Goals;

public class GoalResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid SourceAccountId { get; set; }

    public Guid GoalAccountId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal TargetAmount { get; set; }

    public decimal CurrentAmount { get; set; }

    public decimal ProgressPercent { get; set; }

    public decimal RemainingAmount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public DateOnly? TargetDate { get; set; }

    public GoalStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

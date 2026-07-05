using Current.Api.Common.Enums;

namespace Current.Api.DTOs.Goals;

public class UpdateGoalRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal TargetAmount { get; set; }

    public DateOnly? TargetDate { get; set; }

    public GoalStatus Status { get; set; }
}

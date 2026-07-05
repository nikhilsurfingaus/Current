using Current.Api.DTOs.Goals;
using Current.Api.Entities;

namespace Current.Api.Mappings;

public static class GoalMappings
{
    public static GoalResponse ToResponse(this Goal goal)
    {
        var remainingAmount = Math.Max(0, goal.TargetAmount - goal.CurrentAmount);
        var progressPercent = goal.TargetAmount > 0
            ? Math.Min(100, Math.Round(goal.CurrentAmount / goal.TargetAmount * 100, 2))
            : 0;

        return new GoalResponse
        {
            Id = goal.Id,
            UserId = goal.UserId,
            SourceAccountId = goal.SourceAccountId,
            GoalAccountId = goal.GoalAccountId,
            Name = goal.Name,
            Description = goal.Description,
            TargetAmount = goal.TargetAmount,
            CurrentAmount = goal.CurrentAmount,
            ProgressPercent = progressPercent,
            RemainingAmount = remainingAmount,
            Currency = goal.Currency,
            TargetDate = goal.TargetDate,
            Status = goal.Status,
            IconKey = goal.IconKey,
            CreatedAt = goal.CreatedAt,
            UpdatedAt = goal.UpdatedAt
        };
    }
}

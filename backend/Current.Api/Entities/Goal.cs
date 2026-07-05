using Current.Api.Common.Enums;

namespace Current.Api.Entities;

public class Goal
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid SourceAccountId { get; set; }

    public Guid PotAccountId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal TargetAmount { get; set; }

    public decimal CurrentAmount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public DateOnly? TargetDate { get; set; }

    public GoalStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;

    public Account SourceAccount { get; set; } = null!;

    public Account PotAccount { get; set; } = null!;

    public ICollection<GoalContribution> Contributions { get; set; } = new List<GoalContribution>();
}

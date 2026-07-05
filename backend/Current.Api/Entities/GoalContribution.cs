using Current.Api.Common.Enums;

namespace Current.Api.Entities;

public class GoalContribution
{
    public Guid Id { get; set; }

    public Guid GoalId { get; set; }

    public Guid? TransactionId { get; set; }

    public decimal Amount { get; set; }

    public ContributionType ContributionType { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public Goal Goal { get; set; } = null!;

    public Transaction? Transaction { get; set; }
}

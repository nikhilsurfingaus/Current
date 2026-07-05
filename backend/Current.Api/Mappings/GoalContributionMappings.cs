using Current.Api.DTOs.Goals;
using Current.Api.Entities;

namespace Current.Api.Mappings;

public static class GoalContributionMappings
{
    public static GoalContributionResponse ToResponse(this GoalContribution contribution)
    {
        return new GoalContributionResponse
        {
            Id = contribution.Id,
            GoalId = contribution.GoalId,
            TransactionId = contribution.TransactionId,
            Amount = contribution.Amount,
            ContributionType = contribution.ContributionType,
            Note = contribution.Note,
            CreatedAt = contribution.CreatedAt
        };
    }
}

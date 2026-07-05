using Current.Api.DTOs.Goals;

namespace Current.Api.Interfaces;

public interface IGoalService
{
    Task<IReadOnlyList<GoalResponse>> GetAllGoalsAsync(Guid currentUserId);

    Task<GoalResponse?> GetGoalByIdAsync(Guid goalId, Guid currentUserId);

    Task<GoalResponse> CreateGoalAsync(CreateGoalRequest request, Guid currentUserId);

    Task<GoalResponse?> UpdateGoalAsync(Guid goalId, UpdateGoalRequest request, Guid currentUserId);

    Task<GoalResponse?> CancelGoalAsync(Guid goalId, Guid currentUserId);

    Task<GoalResponse> ContributeToGoalAsync(Guid goalId, ContributeGoalRequest request, Guid currentUserId);

    Task<GoalResponse> WithdrawFromGoalAsync(Guid goalId, WithdrawGoalRequest request, Guid currentUserId);

    Task<IReadOnlyList<GoalContributionResponse>> GetContributionHistoryAsync(Guid goalId, Guid currentUserId);
}

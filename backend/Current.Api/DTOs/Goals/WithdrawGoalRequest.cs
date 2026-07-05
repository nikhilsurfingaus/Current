namespace Current.Api.DTOs.Goals;

public class WithdrawGoalRequest
{
    public decimal Amount { get; set; }

    public Guid DestinationAccountId { get; set; }

    public string? Note { get; set; }
}

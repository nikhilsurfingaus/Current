namespace Current.Api.DTOs.Goals;

public class CreateGoalRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal TargetAmount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public Guid SourceAccountId { get; set; }

    public DateOnly? TargetDate { get; set; }
}

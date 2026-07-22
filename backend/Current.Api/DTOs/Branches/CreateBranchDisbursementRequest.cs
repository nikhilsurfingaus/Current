namespace Current.Api.DTOs.Branches;

public class CreateBranchDisbursementRequest
{
    public string? RecipientEmail { get; set; }

    public Guid? RecipientAccountId { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }
}

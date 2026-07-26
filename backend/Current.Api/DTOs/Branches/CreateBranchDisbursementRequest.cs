namespace Current.Api.DTOs.Branches;

public class CreateBranchDisbursementRequest
{
    public string? RecipientEmail { get; set; }

    public Guid? RecipientAccountId { get; set; }

    public string? RecipientBsb { get; set; }

    public string? RecipientAccountNumber { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }
}

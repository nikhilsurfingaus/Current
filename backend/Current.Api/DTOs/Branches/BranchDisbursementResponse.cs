namespace Current.Api.DTOs.Branches;

public class BranchDisbursementResponse
{
    public Guid TransactionId { get; set; }

    public Guid RecipientAccountId { get; set; }

    public string RecipientEmail { get; set; } = string.Empty;

    public string RecipientName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal TreasuryBalanceAfter { get; set; }

    public DateTime CreatedAt { get; set; }
}

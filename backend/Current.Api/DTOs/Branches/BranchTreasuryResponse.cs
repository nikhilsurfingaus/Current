namespace Current.Api.DTOs.Branches;

public class BranchTreasuryResponse
{
    public Guid BranchId { get; set; }

    public string BranchName { get; set; } = string.Empty;

    public string BranchCode { get; set; } = string.Empty;

    public decimal TreasuryBalance { get; set; }

    public string Currency { get; set; } = string.Empty;
}

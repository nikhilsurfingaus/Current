namespace Current.Api.DTOs.Loans;

public class LoanLimitsResponse
{
    public string Currency { get; set; } = string.Empty;

    public decimal TotalHoldings { get; set; }

    public string TierLabel { get; set; } = string.Empty;

    public decimal MaxSingleLoan { get; set; }

    public decimal MaxTotalOutstanding { get; set; }

    public int MaxOpenLoans { get; set; }

    public int OpenLoanCount { get; set; }

    public decimal CurrentOutstandingExposure { get; set; }

    public decimal AvailableBorrowingCapacity { get; set; }
}

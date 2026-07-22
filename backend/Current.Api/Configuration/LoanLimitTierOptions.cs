namespace Current.Api.Configuration;

public class LoanLimitTierOptions
{
    public decimal MinHoldings { get; set; }

    public string Label { get; set; } = string.Empty;

    public decimal MaxSingleLoan { get; set; }

    public decimal MaxTotalOutstanding { get; set; }

    public int MaxOpenLoans { get; set; }
}

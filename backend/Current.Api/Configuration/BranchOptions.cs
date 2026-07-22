namespace Current.Api.Configuration;

public class BranchOptions
{
    public const string SectionName = "Branch";

    public decimal WelcomeCreditAmount { get; set; }

    public int WelcomeCreditMaxAccounts { get; set; }

    public decimal InitialTreasuryBalance { get; set; }

    public decimal MinLoanAmount { get; set; }

    public decimal MaxLoanAmount { get; set; }

    public decimal DefaultInterestRatePercent { get; set; }

    public int MaxActiveLoansPerUser { get; set; }

    public int MaxTermMonths { get; set; }
}

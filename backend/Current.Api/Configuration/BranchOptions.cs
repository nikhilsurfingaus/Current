namespace Current.Api.Configuration;

public class BranchOptions
{
    public const string SectionName = "Branch";

    public decimal WelcomeCreditAmount { get; set; }

    public int WelcomeCreditMaxAccounts { get; set; }

    public decimal InitialTreasuryBalance { get; set; }
}

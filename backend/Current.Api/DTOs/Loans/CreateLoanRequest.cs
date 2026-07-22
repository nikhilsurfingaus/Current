namespace Current.Api.DTOs.Loans;

public class CreateLoanRequest
{
    public decimal Principal { get; set; }

    public int TermMonths { get; set; }

    public Guid? FundedAccountId { get; set; }

    public string? Purpose { get; set; }
}

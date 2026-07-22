namespace Current.Api.DTOs.Loans;

public class RepayLoanRequest
{
    public decimal Amount { get; set; }

    public Guid SourceAccountId { get; set; }
}

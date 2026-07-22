namespace Current.Api.DTOs.Loans;

public class LoanRepaymentResponse
{
    public Guid Id { get; set; }

    public Guid LoanId { get; set; }

    public Guid? TransactionId { get; set; }

    public decimal Amount { get; set; }

    public decimal PrincipalPortion { get; set; }

    public decimal InterestPortion { get; set; }

    public DateTime CreatedAt { get; set; }
}

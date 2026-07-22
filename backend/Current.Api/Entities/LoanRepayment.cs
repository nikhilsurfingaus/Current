namespace Current.Api.Entities;

public class LoanRepayment
{
    public Guid Id { get; set; }

    public Guid LoanId { get; set; }

    public Guid? TransactionId { get; set; }

    public decimal Amount { get; set; }

    public decimal PrincipalPortion { get; set; }

    public decimal InterestPortion { get; set; }

    public DateTime CreatedAt { get; set; }

    public Loan Loan { get; set; } = null!;

    public Transaction? Transaction { get; set; }
}

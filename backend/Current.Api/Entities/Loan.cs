using Current.Api.Common.Enums;

namespace Current.Api.Entities;

public class Loan
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid BranchId { get; set; }

    public Guid FundedAccountId { get; set; }

    public Guid? DisbursementTransactionId { get; set; }

    public decimal Principal { get; set; }

    public decimal OutstandingPrincipal { get; set; }

    public decimal InterestRatePercent { get; set; }

    public decimal MonthlyPayment { get; set; }

    public string Currency { get; set; } = string.Empty;

    public int TermMonths { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? NextDueDate { get; set; }

    public DateOnly? MaturityDate { get; set; }

    public LoanStatus Status { get; set; }

    public string? Purpose { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;

    public Branch Branch { get; set; } = null!;

    public Account FundedAccount { get; set; } = null!;

    public Transaction? DisbursementTransaction { get; set; }

    public ICollection<LoanRepayment> Repayments { get; set; } = new List<LoanRepayment>();
}

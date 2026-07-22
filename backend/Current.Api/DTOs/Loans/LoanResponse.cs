using Current.Api.Common.Enums;

namespace Current.Api.DTOs.Loans;

public class LoanResponse
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

    public int TimeLeftMonths { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? NextDueDate { get; set; }

    public DateOnly? MaturityDate { get; set; }

    public LoanStatus Status { get; set; }

    public bool IsOverdue { get; set; }

    public string? Purpose { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

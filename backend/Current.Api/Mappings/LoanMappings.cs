using Current.Api.Common.Enums;
using Current.Api.DTOs.Loans;
using Current.Api.Entities;

namespace Current.Api.Mappings;

public static class LoanMappings
{
    public static LoanResponse ToResponse(this Loan loan)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var timeLeftMonths = loan.Status is LoanStatus.Active or LoanStatus.Overdue
            ? Math.Max(0, loan.TermMonths - CountElapsedMonths(loan))
            : loan.Status == LoanStatus.Paid
                ? 0
                : loan.TermMonths;

        var isOverdue = loan.Status == LoanStatus.Active
            && loan.NextDueDate.HasValue
            && loan.NextDueDate.Value < today;

        return new LoanResponse
        {
            Id = loan.Id,
            UserId = loan.UserId,
            BranchId = loan.BranchId,
            FundedAccountId = loan.FundedAccountId,
            DisbursementTransactionId = loan.DisbursementTransactionId,
            Principal = loan.Principal,
            OutstandingPrincipal = loan.OutstandingPrincipal,
            InterestRatePercent = loan.InterestRatePercent,
            MonthlyPayment = loan.MonthlyPayment,
            Currency = loan.Currency,
            TermMonths = loan.TermMonths,
            TimeLeftMonths = timeLeftMonths,
            StartDate = loan.StartDate,
            NextDueDate = loan.NextDueDate,
            MaturityDate = loan.MaturityDate,
            Status = isOverdue ? LoanStatus.Overdue : loan.Status,
            IsOverdue = isOverdue,
            Purpose = loan.Purpose,
            RejectionReason = loan.RejectionReason,
            CreatedAt = loan.CreatedAt,
            UpdatedAt = loan.UpdatedAt
        };
    }

    public static LoanAdminResponse ToAdminResponse(this Loan loan)
    {
        var response = loan.ToResponse();
        return new LoanAdminResponse
        {
            Id = response.Id,
            UserId = response.UserId,
            BranchId = response.BranchId,
            FundedAccountId = response.FundedAccountId,
            DisbursementTransactionId = response.DisbursementTransactionId,
            Principal = response.Principal,
            OutstandingPrincipal = response.OutstandingPrincipal,
            InterestRatePercent = response.InterestRatePercent,
            MonthlyPayment = response.MonthlyPayment,
            Currency = response.Currency,
            TermMonths = response.TermMonths,
            TimeLeftMonths = response.TimeLeftMonths,
            StartDate = response.StartDate,
            NextDueDate = response.NextDueDate,
            MaturityDate = response.MaturityDate,
            Status = response.Status,
            IsOverdue = response.IsOverdue,
            Purpose = response.Purpose,
            RejectionReason = response.RejectionReason,
            CreatedAt = response.CreatedAt,
            UpdatedAt = response.UpdatedAt,
            BorrowerEmail = loan.User.Email,
            BorrowerName = $"{loan.User.FirstName} {loan.User.LastName}".Trim()
        };
    }

    private static int CountElapsedMonths(Loan loan)
    {
        if (!loan.StartDate.HasValue)
        {
            return 0;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var months = (today.Year - loan.StartDate.Value.Year) * 12
            + (today.Month - loan.StartDate.Value.Month);

        if (today.Day < loan.StartDate.Value.Day)
        {
            months -= 1;
        }

        return Math.Max(0, months);
    }
}

using Current.Api.DTOs.Loans;
using Current.Api.Entities;

namespace Current.Api.Mappings;

public static class LoanRepaymentMappings
{
    public static LoanRepaymentResponse ToResponse(this LoanRepayment repayment)
    {
        return new LoanRepaymentResponse
        {
            Id = repayment.Id,
            LoanId = repayment.LoanId,
            TransactionId = repayment.TransactionId,
            Amount = repayment.Amount,
            PrincipalPortion = repayment.PrincipalPortion,
            InterestPortion = repayment.InterestPortion,
            CreatedAt = repayment.CreatedAt
        };
    }
}

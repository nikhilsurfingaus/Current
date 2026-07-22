using Current.Api.Common.Enums;
using Current.Api.DTOs.Loans;

namespace Current.Api.Interfaces;

public interface ILoanService
{
    Task<IReadOnlyList<LoanResponse>> GetUserLoansAsync(Guid currentUserId);

    Task<LoanResponse?> GetUserLoanByIdAsync(Guid loanId, Guid currentUserId);

    Task<LoanResponse> CreateLoanRequestAsync(CreateLoanRequest request, Guid currentUserId);

    Task<LoanResponse?> CancelLoanRequestAsync(Guid loanId, Guid currentUserId);

    Task<LoanResponse> RepayLoanAsync(Guid loanId, RepayLoanRequest request, Guid currentUserId);

    Task<IReadOnlyList<LoanRepaymentResponse>> GetRepaymentHistoryAsync(Guid loanId, Guid currentUserId);

    Task<IReadOnlyList<LoanAdminResponse>> GetLoansForAdminAsync(LoanStatus? status);

    Task<LoanAdminResponse> ApproveLoanAsync(Guid loanId);

    Task<LoanAdminResponse> RejectLoanAsync(Guid loanId, RejectLoanRequest request);
}

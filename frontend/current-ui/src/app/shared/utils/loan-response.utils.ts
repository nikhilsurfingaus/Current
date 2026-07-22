import { Loan, LoanAdmin, LoanRepayment } from '../models/loans/loan.model';
import { parseLoanStatus } from './loan-status.utils';

export function normalizeLoanResponse(loan: Loan): Loan {
  return {
    ...loan,
    status: parseLoanStatus(loan.status),
  };
}

export function normalizeLoanAdminResponse(loan: LoanAdmin): LoanAdmin {
  return {
    ...normalizeLoanResponse(loan),
    borrowerEmail: loan.borrowerEmail,
    borrowerName: loan.borrowerName,
  };
}

export function normalizeLoanRepaymentResponse(repayment: LoanRepayment): LoanRepayment {
  return { ...repayment };
}

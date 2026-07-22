import { LoanStatus } from '../enums';

export interface Loan {
  id: string;
  userId: string;
  branchId: string;
  fundedAccountId: string;
  disbursementTransactionId: string | null;
  principal: number;
  outstandingPrincipal: number;
  interestRatePercent: number;
  monthlyPayment: number;
  currency: string;
  termMonths: number;
  timeLeftMonths: number;
  startDate: string | null;
  nextDueDate: string | null;
  maturityDate: string | null;
  status: LoanStatus;
  isOverdue: boolean;
  purpose: string | null;
  rejectionReason: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface LoanAdmin extends Loan {
  borrowerEmail: string;
  borrowerName: string;
}

export interface LoanRepayment {
  id: string;
  loanId: string;
  transactionId: string | null;
  amount: number;
  principalPortion: number;
  interestPortion: number;
  createdAt: string;
}

export interface LoanLimits {
  currency: string;
  totalHoldings: number;
  tierLabel: string;
  maxSingleLoan: number;
  maxTotalOutstanding: number;
  maxOpenLoans: number;
  openLoanCount: number;
  currentOutstandingExposure: number;
  availableBorrowingCapacity: number;
}

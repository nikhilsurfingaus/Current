import { LoanStatus } from '../models/enums';

export const LOAN_STATUS_LABELS: Record<LoanStatus, string> = {
  [LoanStatus.Pending]: 'Pending',
  [LoanStatus.Active]: 'Active',
  [LoanStatus.Paid]: 'Paid',
  [LoanStatus.Overdue]: 'Overdue',
  [LoanStatus.Defaulted]: 'Defaulted',
  [LoanStatus.Rejected]: 'Rejected',
  [LoanStatus.Cancelled]: 'Cancelled',
};

export const LOAN_STATUS_FILTER_OPTIONS = [
  { value: null, label: 'All' },
  { value: LoanStatus.Pending, label: LOAN_STATUS_LABELS[LoanStatus.Pending] },
  { value: LoanStatus.Active, label: LOAN_STATUS_LABELS[LoanStatus.Active] },
  { value: LoanStatus.Overdue, label: LOAN_STATUS_LABELS[LoanStatus.Overdue] },
  { value: LoanStatus.Paid, label: LOAN_STATUS_LABELS[LoanStatus.Paid] },
  { value: LoanStatus.Rejected, label: LOAN_STATUS_LABELS[LoanStatus.Rejected] },
  { value: LoanStatus.Cancelled, label: LOAN_STATUS_LABELS[LoanStatus.Cancelled] },
] as const;

export function parseLoanStatus(value: unknown): LoanStatus {
  if (typeof value === 'number' && !Number.isNaN(value)) {
    if (value >= LoanStatus.Pending && value <= LoanStatus.Cancelled) {
      return value;
    }
  }

  if (typeof value === 'string') {
    const normalizedStatus = value.trim().toLowerCase();

    if (normalizedStatus === 'pending' || normalizedStatus === '0') {
      return LoanStatus.Pending;
    }

    if (normalizedStatus === 'active' || normalizedStatus === '1') {
      return LoanStatus.Active;
    }

    if (normalizedStatus === 'paid' || normalizedStatus === '2') {
      return LoanStatus.Paid;
    }

    if (normalizedStatus === 'overdue' || normalizedStatus === '3') {
      return LoanStatus.Overdue;
    }

    if (normalizedStatus === 'defaulted' || normalizedStatus === '4') {
      return LoanStatus.Defaulted;
    }

    if (normalizedStatus === 'rejected' || normalizedStatus === '5') {
      return LoanStatus.Rejected;
    }

    if (normalizedStatus === 'cancelled' || normalizedStatus === '6') {
      return LoanStatus.Cancelled;
    }
  }

  return LoanStatus.Pending;
}

export function getLoanStatusLabel(loanStatus: LoanStatus): string {
  return LOAN_STATUS_LABELS[loanStatus] ?? 'Unknown';
}

export function isLoanRepayable(loanStatus: LoanStatus): boolean {
  return loanStatus === LoanStatus.Active || loanStatus === LoanStatus.Overdue;
}

export function isLoanCancellable(loanStatus: LoanStatus): boolean {
  return loanStatus === LoanStatus.Pending;
}

export function getLoanRepaymentProgressPercent(loan: {
  principal: number;
  outstandingPrincipal: number;
}): number {
  if (loan.principal <= 0) {
    return 0;
  }

  const paidPrincipal = loan.principal - loan.outstandingPrincipal;
  return Math.min(100, Math.round((paidPrincipal / loan.principal) * 100));
}

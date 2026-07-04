import { TransactionStatus } from '../models/enums';

export const TRANSACTION_STATUS_LABELS: Record<TransactionStatus, string> = {
  [TransactionStatus.Pending]: 'Pending',
  [TransactionStatus.Completed]: 'Completed',
  [TransactionStatus.Failed]: 'Failed',
  [TransactionStatus.Reversed]: 'Reversed',
};

export function getTransactionStatusLabel(status: TransactionStatus): string {
  return TRANSACTION_STATUS_LABELS[status] ?? 'Unknown';
}

import { TransactionStatus } from '../enums';
import { LedgerEntry } from './ledger-entry.model';

export interface Transaction {
  id: string;
  fromAccountId: string;
  toAccountId: string;
  amount: number;
  description: string;
  status: TransactionStatus;
  createdAt: string;
  ledgerEntries: LedgerEntry[];
}

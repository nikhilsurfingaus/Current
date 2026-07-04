import { LedgerEntryType } from '../enums';

export interface LedgerEntry {
  id: string;
  accountId: string;
  entryType: LedgerEntryType;
  amount: number;
  createdAt: string;
}

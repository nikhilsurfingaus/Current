import { AccountType } from '../enums';

export interface Account {
  id: string;
  userId: string;
  name: string;
  accountType: AccountType;
  currentBalance: number;
  currency: string;
  createdAt: string;
  updatedAt: string;
}

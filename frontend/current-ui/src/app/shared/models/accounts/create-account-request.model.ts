import { AccountType } from '../enums';

export interface CreateAccountRequest {
  name: string;
  accountType: AccountType;
  currentBalance: number;
  currency: string;
}

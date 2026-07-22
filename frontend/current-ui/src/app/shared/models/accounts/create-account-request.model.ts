import { AccountType } from '../enums';

export interface CreateAccountRequest {
  name: string;
  accountType: AccountType;
  currency: string;
}

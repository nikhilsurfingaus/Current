import { AccountType } from '../models/enums';

export const ACCOUNT_TYPE_LABELS: Record<AccountType, string> = {
  [AccountType.Everyday]: 'Everyday',
  [AccountType.Savings]: 'Savings',
  [AccountType.Investment]: 'Investment',
};

export const ACCOUNT_TYPE_OPTIONS = [
  { value: AccountType.Everyday, label: ACCOUNT_TYPE_LABELS[AccountType.Everyday] },
  { value: AccountType.Savings, label: ACCOUNT_TYPE_LABELS[AccountType.Savings] },
  { value: AccountType.Investment, label: ACCOUNT_TYPE_LABELS[AccountType.Investment] },
];

export function getAccountTypeLabel(accountType: AccountType): string {
  return ACCOUNT_TYPE_LABELS[accountType] ?? 'Unknown';
}

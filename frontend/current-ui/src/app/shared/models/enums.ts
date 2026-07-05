export enum AccountType {
  Everyday = 0,
  Savings = 1,
  Investment = 2,
}

export enum UserRole {
  User = 0,
  Admin = 1,
}

export enum TransactionStatus {
  Pending = 0,
  Completed = 1,
  Failed = 2,
  Reversed = 3,
}

export enum LedgerEntryType {
  Debit = 0,
  Credit = 1,
}

export enum GoalStatus {
  Active = 0,
  Completed = 1,
  Cancelled = 2,
}

export enum ContributionType {
  Deposit = 0,
  Withdrawal = 1,
  Adjustment = 2,
}

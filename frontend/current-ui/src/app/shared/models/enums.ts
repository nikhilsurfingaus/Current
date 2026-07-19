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

export enum TransactionCategory {
  Income = 0,
  Transfer = 1,
  Housing = 2,
  Groceries = 3,
  Fuel = 4,
  Food = 5,
  Shopping = 6,
  Entertainment = 7,
  Bills = 8,
  Investment = 9,
  Travel = 10,
  Health = 11,
  Education = 12,
  Other = 13,
}

export enum PaymentDirection {
  Sent = 0,
  Received = 1,
}

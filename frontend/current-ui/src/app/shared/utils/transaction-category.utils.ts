import { TransactionCategory } from '../models/enums';

export const TRANSACTION_CATEGORY_LABELS: Record<TransactionCategory, string> = {
  [TransactionCategory.Income]: 'Income',
  [TransactionCategory.Transfer]: 'Transfer',
  [TransactionCategory.Housing]: 'Housing',
  [TransactionCategory.Groceries]: 'Groceries',
  [TransactionCategory.Fuel]: 'Fuel',
  [TransactionCategory.Food]: 'Food',
  [TransactionCategory.Shopping]: 'Shopping',
  [TransactionCategory.Entertainment]: 'Entertainment',
  [TransactionCategory.Bills]: 'Bills',
  [TransactionCategory.Investment]: 'Investment',
  [TransactionCategory.Travel]: 'Travel',
  [TransactionCategory.Health]: 'Health',
  [TransactionCategory.Education]: 'Education',
  [TransactionCategory.Other]: 'Other',
};

export function getTransactionCategoryLabel(category: TransactionCategory): string {
  return TRANSACTION_CATEGORY_LABELS[category] ?? 'Unknown';
}

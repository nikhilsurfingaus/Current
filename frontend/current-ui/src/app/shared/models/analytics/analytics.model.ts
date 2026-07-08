import { TransactionCategory } from '../enums';

export interface AnalyticsOverview {
  totalBalance: number;
  monthlyIncome: number;
  monthlyExpenses: number;
  netCashFlow: number;
  savingsRatePercent: number;
  activeGoalsCount: number;
  completedGoalsCount: number;
  totalGoalSavings: number;
}

export interface CashFlowMonthPoint {
  month: string;
  income: number;
  expenses: number;
  net: number;
}

export interface CashFlowResponse {
  months: CashFlowMonthPoint[];
}

export interface NetWorthHistoryPoint {
  date: string;
  balance: number;
}

export interface NetWorthHistoryResponse {
  points: NetWorthHistoryPoint[];
}

export interface CategoryBreakdownItem {
  category: TransactionCategory;
  amount: number;
  percent: number;
}

export interface CategoryBreakdownResponse {
  categories: CategoryBreakdownItem[];
}

export interface GoalAnalyticsItem {
  goalId: string;
  name: string;
  targetAmount: number;
  currentAmount: number;
  completionPercent: number;
  remainingAmount: number;
  projectedCompletionDate: string | null;
}

export interface GoalProgressResponse {
  totalSaved: number;
  totalRemaining: number;
  goals: GoalAnalyticsItem[];
}

export interface MonthlySummaryResponse {
  transactionCount: number;
  income: number;
  expenses: number;
  transfers: number;
  averageTransactionAmount: number;
  largestExpense: number;
  largestIncome: number;
}

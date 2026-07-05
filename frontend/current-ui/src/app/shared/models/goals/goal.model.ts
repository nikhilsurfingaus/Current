import { ContributionType, GoalStatus } from '../enums';

export interface Goal {
  id: string;
  userId: string;
  sourceAccountId: string;
  goalAccountId: string;
  name: string;
  description: string | null;
  targetAmount: number;
  currentAmount: number;
  progressPercent: number;
  remainingAmount: number;
  currency: string;
  targetDate: string | null;
  status: GoalStatus;
  iconKey: string;
  createdAt: string;
  updatedAt: string;
}

export interface GoalContribution {
  id: string;
  goalId: string;
  transactionId: string | null;
  amount: number;
  contributionType: ContributionType;
  note: string | null;
  createdAt: string;
}

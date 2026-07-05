export interface CreateGoalRequest {
  name: string;
  description?: string | null;
  targetAmount: number;
  currency: string;
  sourceAccountId: string;
  targetDate?: string | null;
  iconKey?: string | null;
}

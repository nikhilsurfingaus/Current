import { GoalStatus } from '../enums';

export interface UpdateGoalRequest {
  name: string;
  description?: string | null;
  targetAmount: number;
  targetDate?: string | null;
  status: GoalStatus;
  iconKey?: string | null;
}

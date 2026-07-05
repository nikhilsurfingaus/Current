import { GoalStatus } from '../models/enums';

export const GOAL_STATUS_LABELS: Record<GoalStatus, string> = {
  [GoalStatus.Active]: 'Active',
  [GoalStatus.Completed]: 'Completed',
  [GoalStatus.Cancelled]: 'Cancelled',
};

export const GOAL_STATUS_FILTER_OPTIONS = [
  { value: null, label: 'All' },
  { value: GoalStatus.Active, label: GOAL_STATUS_LABELS[GoalStatus.Active] },
  { value: GoalStatus.Completed, label: GOAL_STATUS_LABELS[GoalStatus.Completed] },
  { value: GoalStatus.Cancelled, label: GOAL_STATUS_LABELS[GoalStatus.Cancelled] },
] as const;

export function getGoalStatusLabel(goalStatus: GoalStatus): string {
  return GOAL_STATUS_LABELS[goalStatus] ?? 'Unknown';
}

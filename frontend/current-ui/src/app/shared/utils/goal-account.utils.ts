import { Account } from '../models';
import { Goal } from '../models/goals/goal.model';

export function getGoalAccountIds(goals: Goal[]): Set<string> {
  return new Set(goals.map((goal) => goal.goalAccountId));
}

export function filterNonGoalAccounts(accounts: Account[], goals: Goal[]): Account[] {
  const goalAccountIds = getGoalAccountIds(goals);

  return accounts.filter((account) => !goalAccountIds.has(account.id));
}

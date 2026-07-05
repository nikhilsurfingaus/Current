import { ContributionType } from '../models/enums';

export const CONTRIBUTION_TYPE_LABELS: Record<ContributionType, string> = {
  [ContributionType.Deposit]: 'Deposit',
  [ContributionType.Withdrawal]: 'Withdrawal',
  [ContributionType.Adjustment]: 'Adjustment',
};

export function getContributionTypeLabel(contributionType: ContributionType): string {
  return CONTRIBUTION_TYPE_LABELS[contributionType] ?? 'Unknown';
}

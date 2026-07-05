export const DEFAULT_GOAL_ICON_KEY = 'default';

export interface GoalIconOption {
  key: string;
  label: string;
  backgroundColor: string;
  iconColor: string;
}

export const GOAL_ICON_OPTIONS: GoalIconOption[] = [
  {
    key: DEFAULT_GOAL_ICON_KEY,
    label: 'Savings',
    backgroundColor: '#eff6ff',
    iconColor: '#2f80ed',
  },
  {
    key: 'vacation',
    label: 'Vacation',
    backgroundColor: '#ecfeff',
    iconColor: '#0891b2',
  },
  {
    key: 'home',
    label: 'Home',
    backgroundColor: '#f0fdf4',
    iconColor: '#16a34a',
  },
  {
    key: 'emergency',
    label: 'Emergency',
    backgroundColor: '#fef3c7',
    iconColor: '#d97706',
  },
  {
    key: 'car',
    label: 'Car',
    backgroundColor: '#eef2ff',
    iconColor: '#4f46e5',
  },
  {
    key: 'gaming',
    label: 'Gaming',
    backgroundColor: '#fae8ff',
    iconColor: '#a855f7',
  },
  {
    key: 'investment',
    label: 'Investment',
    backgroundColor: '#ecfdf5',
    iconColor: '#059669',
  },
  {
    key: 'education',
    label: 'Education',
    backgroundColor: '#fff7ed',
    iconColor: '#ea580c',
  },
];

export function resolveGoalIconOption(iconKey?: string | null): GoalIconOption {
  const normalizedIconKey = iconKey?.trim().toLowerCase() ?? DEFAULT_GOAL_ICON_KEY;
  const matchedOption = GOAL_ICON_OPTIONS.find((option) => option.key === normalizedIconKey);

  return matchedOption ?? GOAL_ICON_OPTIONS[0];
}

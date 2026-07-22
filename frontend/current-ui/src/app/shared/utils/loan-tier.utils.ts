export function getLoanTierSlug(tierLabel: string): string {
  return tierLabel.trim().toLowerCase();
}

export function getLoanTierEmoji(tierLabel: string): string {
  switch (getLoanTierSlug(tierLabel)) {
    case 'bronze':
      return '🥉';
    case 'silver':
      return '🥈';
    case 'gold':
      return '🥇';
    case 'platinum':
      return '💎';
    default:
      return '🌱';
  }
}

export interface CurrencyBalanceTotal {
  currency: string;
  totalBalance: number;
}

export function buildCurrencyBalanceTotals(
  accounts: { currency: string; currentBalance: number }[],
): CurrencyBalanceTotal[] {
  const totalsByCurrency = new Map<string, number>();

  for (const account of accounts) {
    const currency = account.currency.toUpperCase();
    const existingTotal = totalsByCurrency.get(currency) ?? 0;
    totalsByCurrency.set(currency, existingTotal + account.currentBalance);
  }

  return [...totalsByCurrency.entries()]
    .map(([currency, totalBalance]) => ({ currency, totalBalance }))
    .sort((left, right) => left.currency.localeCompare(right.currency));
}

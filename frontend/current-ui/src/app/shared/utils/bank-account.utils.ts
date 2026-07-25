export function formatBankAccountLine(bsb: string, accountNumber: string): string {
  return `${bsb} · ${accountNumber}`;
}

export const BRANCH_WELCOME_CREDIT_DESCRIPTION = 'Welcome credit from Current HQ';

export function isBranchFundedTransaction(description: string): boolean {
  const normalizedDescription = description.trim().toLowerCase();
  return normalizedDescription.includes('current hq')
    || normalizedDescription.includes('welcome credit');
}

export function getBranchTransactionLabel(description: string): string {
  if (isBranchFundedTransaction(description)) {
    return 'Funded by Current Branch';
  }

  return description || '—';
}

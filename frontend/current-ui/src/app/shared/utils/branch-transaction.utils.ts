export const BRANCH_WELCOME_CREDIT_DESCRIPTION = 'Welcome credit from Current HQ';
export const CURRENT_BRANCH_FROM_LABEL = 'Funded by Current Branch';
export const HQ_TREASURY_ACCOUNT_ID = '22222222-2222-2222-2222-222222222222';

export interface BranchTransactionLike {
  description: string;
  reference?: string | null;
  fromAccountId?: string;
}

export function isBranchFundedTransaction(transaction: BranchTransactionLike): boolean {
  if (transaction.fromAccountId === HQ_TREASURY_ACCOUNT_ID) {
    return true;
  }

  const normalizedReference = transaction.reference?.trim().toUpperCase() ?? '';
  if (normalizedReference.startsWith('BRANCH-')) {
    return true;
  }

  const normalizedDescription = transaction.description.trim().toLowerCase();
  return normalizedDescription.includes('current hq')
    || normalizedDescription.includes('welcome credit')
    || normalizedDescription.includes('branch top-up');
}

export function getBranchTransactionLabel(transaction: BranchTransactionLike): string {
  if (isBranchFundedTransaction(transaction)) {
    return CURRENT_BRANCH_FROM_LABEL;
  }

  return transaction.description || '—';
}

export function getTransactionFromDisplayName(
  transaction: BranchTransactionLike & { fromAccountId: string },
  resolveAccountName: (accountId: string) => string,
): string {
  if (isBranchFundedTransaction(transaction)) {
    return CURRENT_BRANCH_FROM_LABEL;
  }

  return resolveAccountName(transaction.fromAccountId);
}

export function getLedgerAccountDisplayName(
  accountId: string,
  transaction: BranchTransactionLike & { fromAccountId: string },
  resolveAccountName: (accountId: string) => string,
): string {
  const accountName = resolveAccountName(accountId);
  if (accountName !== 'Unknown account') {
    return accountName;
  }

  if (isBranchFundedTransaction(transaction) && accountId === transaction.fromAccountId) {
    return CURRENT_BRANCH_FROM_LABEL;
  }

  return accountName;
}

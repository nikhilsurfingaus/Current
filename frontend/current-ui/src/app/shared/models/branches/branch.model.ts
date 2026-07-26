export interface BranchTreasury {
  branchId: string;
  branchName: string;
  branchCode: string;
  treasuryBalance: number;
  currency: string;
}

export interface CreateBranchDisbursementRequest {
  recipientEmail?: string;
  recipientAccountId?: string;
  recipientBsb?: string;
  recipientAccountNumber?: string;
  amount: number;
  description?: string;
}

export interface BranchDisbursement {
  transactionId: string;
  recipientAccountId: string;
  recipientEmail: string;
  recipientName: string;
  amount: number;
  currency: string;
  description: string;
  treasuryBalanceAfter: number;
  createdAt: string;
}

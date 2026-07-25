import { PaymentDirection, TransactionStatus } from '../enums';

export interface PaymentReceipt {
  transactionId: string;
  fromAccountId: string;
  recipientAccountId: string;
  recipientAccountName: string;
  recipientName: string;
  recipientEmail?: string | null;
  recipientBsb: string;
  recipientAccountNumber: string;
  amount: number;
  currency: string;
  reference: string | null;
  status: TransactionStatus;
  createdAt: string;
}

export interface PaymentHistoryItem {
  transactionId: string;
  direction: PaymentDirection;
  fromAccountId: string;
  toAccountId: string;
  senderName: string;
  senderEmail: string;
  recipientName: string;
  recipientEmail?: string | null;
  recipientBsb: string;
  recipientAccountNumber: string;
  recipientAccountName: string;
  amount: number;
  currency: string;
  reference: string | null;
  status: TransactionStatus;
  createdAt: string;
}

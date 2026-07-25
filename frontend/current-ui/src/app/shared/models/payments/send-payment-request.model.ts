export interface SendPaymentRequest {
  fromAccountId: string;
  recipientEmail?: string | null;
  recipientBsb?: string | null;
  recipientAccountNumber?: string | null;
  amount: number;
  reference?: string | null;
}

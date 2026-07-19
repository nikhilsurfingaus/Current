export interface SendPaymentRequest {
  fromAccountId: string;
  recipientEmail: string;
  amount: number;
  reference?: string | null;
}

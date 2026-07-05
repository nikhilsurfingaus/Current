export interface WithdrawGoalRequest {
  amount: number;
  destinationAccountId: string;
  note?: string | null;
}

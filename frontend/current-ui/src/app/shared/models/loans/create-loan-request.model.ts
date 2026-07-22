export interface CreateLoanRequest {
  principal: number;
  termMonths: number;
  fundedAccountId?: string;
  purpose?: string;
}

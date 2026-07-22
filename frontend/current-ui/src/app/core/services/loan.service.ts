import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';

import { API_PATHS } from '../config/api-paths';
import { ApiService } from './api.service';
import {
  CreateLoanRequest,
  Loan,
  LoanRepayment,
  RepayLoanRequest,
} from '../../shared/models';
import {
  normalizeLoanRepaymentResponse,
  normalizeLoanResponse,
} from '../../shared/utils/loan-response.utils';

@Injectable({
  providedIn: 'root',
})
export class LoanService {
  constructor(private apiService: ApiService) {}

  getAllLoans(): Observable<Loan[]> {
    return this.apiService
      .get<Loan[]>(API_PATHS.loans.list)
      .pipe(map((loans) => loans.map((loan) => normalizeLoanResponse(loan))));
  }

  getLoanById(loanId: string): Observable<Loan> {
    return this.apiService
      .get<Loan>(API_PATHS.loans.byId(loanId))
      .pipe(map((loan) => normalizeLoanResponse(loan)));
  }

  createLoanRequest(createLoanRequest: CreateLoanRequest): Observable<Loan> {
    return this.apiService
      .post<Loan>(API_PATHS.loans.list, createLoanRequest)
      .pipe(map((loan) => normalizeLoanResponse(loan)));
  }

  cancelLoanRequest(loanId: string): Observable<Loan> {
    return this.apiService
      .delete<Loan>(API_PATHS.loans.byId(loanId))
      .pipe(map((loan) => normalizeLoanResponse(loan)));
  }

  repayLoan(loanId: string, repayLoanRequest: RepayLoanRequest): Observable<Loan> {
    return this.apiService
      .post<Loan>(API_PATHS.loans.repay(loanId), repayLoanRequest)
      .pipe(map((loan) => normalizeLoanResponse(loan)));
  }

  getRepaymentHistory(loanId: string): Observable<LoanRepayment[]> {
    return this.apiService
      .get<LoanRepayment[]>(API_PATHS.loans.repayments(loanId))
      .pipe(map((repayments) => repayments.map((repayment) => normalizeLoanRepaymentResponse(repayment))));
  }
}

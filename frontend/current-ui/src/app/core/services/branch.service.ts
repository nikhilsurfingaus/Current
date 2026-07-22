import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';

import { API_PATHS } from '../config/api-paths';
import { ApiService } from './api.service';
import { LoanStatus } from '../../shared/models/enums';
import { RejectLoanRequest } from '../../shared/models/loans/reject-loan-request.model';
import {
  BranchDisbursement,
  BranchTreasury,
  CreateBranchDisbursementRequest,
} from '../../shared/models/branches/branch.model';
import { LoanAdmin } from '../../shared/models/loans/loan.model';
import { normalizeLoanAdminResponse } from '../../shared/utils/loan-response.utils';

@Injectable({
  providedIn: 'root',
})
export class BranchService {
  constructor(private apiService: ApiService) {}

  getTreasury(): Observable<BranchTreasury> {
    return this.apiService.get<BranchTreasury>(API_PATHS.branch.treasury);
  }

  createDisbursement(request: CreateBranchDisbursementRequest): Observable<BranchDisbursement> {
    return this.apiService.post<BranchDisbursement>(API_PATHS.branch.disbursements, request);
  }

  getLoans(loanStatus?: LoanStatus): Observable<LoanAdmin[]> {
    const loansPath = loanStatus !== undefined
      ? `${API_PATHS.branch.loans}?status=${LoanStatus[loanStatus]}`
      : API_PATHS.branch.loans;

    return this.apiService
      .get<LoanAdmin[]>(loansPath)
      .pipe(map((loans) => loans.map((loan) => normalizeLoanAdminResponse(loan))));
  }

  approveLoan(loanId: string): Observable<LoanAdmin> {
    return this.apiService
      .post<LoanAdmin>(API_PATHS.branch.approveLoan(loanId), {})
      .pipe(map((loan) => normalizeLoanAdminResponse(loan)));
  }

  rejectLoan(loanId: string, request: RejectLoanRequest): Observable<LoanAdmin> {
    return this.apiService
      .post<LoanAdmin>(API_PATHS.branch.rejectLoan(loanId), request)
      .pipe(map((loan) => normalizeLoanAdminResponse(loan)));
  }
}

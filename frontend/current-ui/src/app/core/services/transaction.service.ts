import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_PATHS } from '../config/api-paths';
import { ApiService } from './api.service';
import { Transaction, TransferRequest } from '../../shared/models';

@Injectable({
  providedIn: 'root',
})
export class TransactionService {
  constructor(private apiService: ApiService) {}

  getAllTransactions(): Observable<Transaction[]> {
    return this.apiService.get<Transaction[]>(API_PATHS.transactions.list);
  }

  getTransactionById(transactionId: string): Observable<Transaction> {
    return this.apiService.get<Transaction>(API_PATHS.transactions.byId(transactionId));
  }

  transferFunds(transferRequest: TransferRequest): Observable<Transaction> {
    return this.apiService.post<Transaction>(API_PATHS.transactions.transfer, transferRequest);
  }
}

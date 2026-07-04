import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_PATHS } from '../config/api-paths';
import { ApiService } from './api.service';
import { Account, CreateAccountRequest } from '../../shared/models';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  constructor(private apiService: ApiService) {}

  getAllAccounts(): Observable<Account[]> {
    return this.apiService.get<Account[]>(API_PATHS.accounts.list);
  }

  getAccountById(accountId: string): Observable<Account> {
    return this.apiService.get<Account>(API_PATHS.accounts.byId(accountId));
  }

  createAccount(createAccountRequest: CreateAccountRequest): Observable<Account> {
    return this.apiService.post<Account>(API_PATHS.accounts.list, createAccountRequest);
  }
}

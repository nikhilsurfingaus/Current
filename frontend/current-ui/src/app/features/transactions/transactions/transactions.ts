import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { AccountService } from '../../../core/services/account.service';
import { TransactionService } from '../../../core/services/transaction.service';
import { Account, ApiError, LedgerEntryType, Transaction, TransactionStatus } from '../../../shared/models';
import { getTransactionStatusLabel } from '../../../shared/utils/transaction-status.utils';

@Component({
  selector: 'app-transactions',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './transactions.html',
  styleUrl: './transactions.scss',
})
export class TransactionsComponent implements OnInit {
  transactions = signal<Transaction[]>([]);
  accountsById = signal<Record<string, Account>>({});
  transactionsLoading = signal(false);
  transactionsLoadError = signal('');
  expandedTransactionId = signal<string | null>(null);

  readonly getTransactionStatusLabel = getTransactionStatusLabel;
  readonly TransactionStatus = TransactionStatus;
  readonly LedgerEntryType = LedgerEntryType;

  constructor(
    private transactionService: TransactionService,
    private accountService: AccountService,
  ) {}

  ngOnInit(): void {
    this.loadTransactionsPageData();
  }

  loadTransactionsPageData(): void {
    this.transactionsLoading.set(true);
    this.transactionsLoadError.set('');

    this.accountService.getAllAccounts().subscribe({
      next: (accounts) => {
        const accountMap = accounts.reduce<Record<string, Account>>((accumulator, account) => {
          accumulator[account.id] = account;
          return accumulator;
        }, {});
        this.accountsById.set(accountMap);
        this.loadTransactions();
      },
      error: (error: HttpErrorResponse) => {
        this.transactionsLoading.set(false);
        this.transactionsLoadError.set(
          this.resolveErrorMessage(error, 'Unable to load accounts.'),
        );
      },
    });
  }

  loadTransactions(): void {
    this.transactionService.getAllTransactions().subscribe({
      next: (transactions) => {
        this.transactions.set(transactions);
        this.transactionsLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.transactionsLoading.set(false);
        this.transactionsLoadError.set(
          this.resolveErrorMessage(error, 'Unable to load transactions.'),
        );
      },
    });
  }

  getAccountName(accountId: string): string {
    return this.accountsById()[accountId]?.name ?? 'Unknown account';
  }

  getAccountCurrency(accountId: string): string {
    return this.accountsById()[accountId]?.currency ?? 'AUD';
  }

  toggleTransactionDetails(transactionId: string): void {
    if (this.expandedTransactionId() === transactionId) {
      this.expandedTransactionId.set(null);
      return;
    }

    this.expandedTransactionId.set(transactionId);
  }

  isTransactionExpanded(transactionId: string): boolean {
    return this.expandedTransactionId() === transactionId;
  }

  private resolveErrorMessage(error: HttpErrorResponse, fallbackMessage: string): string {
    const apiError = error.error as ApiError | undefined;
    if (apiError?.message) {
      return apiError.message;
    }

    return fallbackMessage;
  }
}

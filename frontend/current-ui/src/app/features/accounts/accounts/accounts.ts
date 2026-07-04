import { CurrencyPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';

import { AccountService } from '../../../core/services/account.service';
import { Account, AccountType, ApiError, CreateAccountRequest } from '../../../shared/models';
import {
  ACCOUNT_TYPE_OPTIONS,
  getAccountTypeLabel,
} from '../../../shared/utils/account-type.utils';

@Component({
  selector: 'app-accounts',
  standalone: true,
  imports: [ReactiveFormsModule, CurrencyPipe],
  templateUrl: './accounts.html',
  styleUrl: './accounts.scss',
})
export class AccountsComponent implements OnInit {
  accounts: Account[] = [];
  accountsLoading = false;
  accountsLoadError = '';
  createPanelOpen = false;
  createFormSubmitted = false;
  createRequestInFlight = false;
  createErrorMessage = '';

  readonly accountTypeOptions = ACCOUNT_TYPE_OPTIONS;
  readonly getAccountTypeLabel = getAccountTypeLabel;

  createAccountForm = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    accountType: new FormControl(AccountType.Everyday, {
      nonNullable: true,
      validators: [Validators.required],
    }),
    currentBalance: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0)],
    }),
    currency: new FormControl('AUD', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(3), Validators.maxLength(3)],
    }),
  });

  constructor(private accountService: AccountService) {}

  ngOnInit(): void {
    this.loadAccounts();
  }

  loadAccounts(): void {
    this.accountsLoading = true;
    this.accountsLoadError = '';

    this.accountService.getAllAccounts().subscribe({
      next: (accounts) => {
        this.accounts = accounts;
        this.accountsLoading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.accountsLoading = false;
        this.accountsLoadError = this.resolveErrorMessage(error, 'Unable to load accounts.');
      },
    });
  }

  openCreatePanel(): void {
    this.createPanelOpen = true;
    this.createFormSubmitted = false;
    this.createErrorMessage = '';
    this.createAccountForm.reset({
      name: '',
      accountType: AccountType.Everyday,
      currentBalance: 0,
      currency: 'AUD',
    });
  }

  closeCreatePanel(): void {
    this.createPanelOpen = false;
    this.createFormSubmitted = false;
    this.createErrorMessage = '';
  }

  onCreateAccount(): void {
    this.createFormSubmitted = true;
    this.createErrorMessage = '';

    if (this.createAccountForm.invalid) {
      return;
    }

    const formValues = this.createAccountForm.getRawValue();
    const createAccountRequest: CreateAccountRequest = {
      name: formValues.name.trim(),
      accountType: formValues.accountType,
      currentBalance: formValues.currentBalance,
      currency: formValues.currency.trim().toUpperCase(),
    };

    this.createRequestInFlight = true;

    this.accountService.createAccount(createAccountRequest).subscribe({
      next: (createdAccount) => {
        this.createRequestInFlight = false;
        this.accounts = [...this.accounts, createdAccount].sort((left, right) =>
          left.name.localeCompare(right.name),
        );
        this.closeCreatePanel();
      },
      error: (error: HttpErrorResponse) => {
        this.createRequestInFlight = false;
        this.createErrorMessage = this.resolveErrorMessage(
          error,
          'Unable to create account.',
        );
      },
    });
  }

  private resolveErrorMessage(error: HttpErrorResponse, fallbackMessage: string): string {
    const apiError = error.error as ApiError | undefined;
    if (apiError?.message) {
      return apiError.message;
    }

    return fallbackMessage;
  }
}

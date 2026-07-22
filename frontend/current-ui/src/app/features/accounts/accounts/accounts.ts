import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';

import { AccountService } from '../../../core/services/account.service';
import { GoalService } from '../../../core/services/goal.service';
import { NormalizeAmountDirective } from '../../../shared/directives/normalize-amount.directive';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state';
import { SkeletonLoaderComponent } from '../../../shared/components/skeleton-loader/skeleton-loader';
import { Account, AccountType, ApiError, CreateAccountRequest, Goal } from '../../../shared/models';
import {
  ACCOUNT_TYPE_OPTIONS,
  getAccountTypeLabel,
} from '../../../shared/utils/account-type.utils';
import { filterNonGoalAccounts } from '../../../shared/utils/goal-account.utils';
import { focusFirstInvalidControl } from '../../../shared/utils/form-accessibility.utils';

@Component({
  selector: 'app-accounts',
  standalone: true,
  imports: [ReactiveFormsModule, CurrencyPipe, NormalizeAmountDirective, SkeletonLoaderComponent, EmptyStateComponent],
  templateUrl: './accounts.html',
  styleUrl: './accounts.scss',
})
export class AccountsComponent implements OnInit {
  accounts = signal<Account[]>([]);
  goals = signal<Goal[]>([]);
  accountsLoading = signal(false);
  accountsLoadError = signal('');
  createPanelOpen = signal(false);
  createFormSubmitted = signal(false);
  createRequestInFlight = signal(false);
  createErrorMessage = signal('');

  readonly accountTypeOptions = ACCOUNT_TYPE_OPTIONS;
  readonly getAccountTypeLabel = getAccountTypeLabel;

  userFacingAccounts = computed(() => filterNonGoalAccounts(this.accounts(), this.goals()));

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

  constructor(
    private accountService: AccountService,
    private goalService: GoalService,
  ) {}

  ngOnInit(): void {
    this.loadAccounts();
  }

  loadAccounts(): void {
    this.accountsLoading.set(true);
    this.accountsLoadError.set('');

    this.accountService.getAllAccounts().subscribe({
      next: (accounts) => {
        this.accounts.set(accounts);
        this.goalService.getAllGoals().subscribe({
          next: (goals) => {
            this.goals.set(goals);
            this.accountsLoading.set(false);
          },
          error: (error: HttpErrorResponse) => {
            this.accountsLoading.set(false);
            this.accountsLoadError.set(
              this.resolveErrorMessage(error, 'Unable to load goals.'),
            );
          },
        });
      },
      error: (error: HttpErrorResponse) => {
        this.accountsLoading.set(false);
        this.accountsLoadError.set(
          this.resolveErrorMessage(error, 'Unable to load accounts.'),
        );
      },
    });
  }

  openCreatePanel(): void {
    this.createPanelOpen.set(true);
    this.createFormSubmitted.set(false);
    this.createErrorMessage.set('');
    this.createAccountForm.reset({
      name: '',
      accountType: AccountType.Everyday,
      currentBalance: 0,
      currency: 'AUD',
    });
  }

  closeCreatePanel(): void {
    this.createPanelOpen.set(false);
    this.createFormSubmitted.set(false);
    this.createErrorMessage.set('');
  }

  onCreateAccount(): void {
    this.createFormSubmitted.set(true);
    this.createErrorMessage.set('');

    if (this.createAccountForm.invalid) {
      focusFirstInvalidControl(this.createAccountForm);
      return;
    }

    const formValues = this.createAccountForm.getRawValue();
    const createAccountRequest: CreateAccountRequest = {
      name: formValues.name.trim(),
      accountType: formValues.accountType,
      currentBalance: formValues.currentBalance,
      currency: formValues.currency.trim().toUpperCase(),
    };

    this.createRequestInFlight.set(true);

    this.accountService.createAccount(createAccountRequest).subscribe({
      next: (createdAccount) => {
        this.createRequestInFlight.set(false);
        this.closeCreatePanel();
        this.loadAccounts();
      },
      error: (error: HttpErrorResponse) => {
        this.createRequestInFlight.set(false);
        this.createErrorMessage.set(
          this.resolveErrorMessage(error, 'Unable to create account.'),
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

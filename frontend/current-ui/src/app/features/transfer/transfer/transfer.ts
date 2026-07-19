import { CurrencyPipe } from '@angular/common';
import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Subscription } from 'rxjs';

import { AccountService } from '../../../core/services/account.service';
import { ToastService } from '../../../core/services/toast.service';
import { TransactionService } from '../../../core/services/transaction.service';
import { NormalizeAmountDirective } from '../../../shared/directives/normalize-amount.directive';
import { Account, ApiError, TransferRequest } from '../../../shared/models';
import { differentAccountsValidator } from './different-accounts.validator';

@Component({
  selector: 'app-transfer',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe, NormalizeAmountDirective],
  templateUrl: './transfer.html',
  styleUrl: './transfer.scss',
})
export class TransferComponent implements OnInit, OnDestroy {
  accounts = signal<Account[]>([]);
  accountsLoading = signal(false);
  accountsLoadError = signal('');
  transferFormSubmitted = signal(false);
  transferRequestInFlight = signal(false);
  transferErrorMessage = signal('');

  private transferFormSubscriptions = new Subscription();

  transferForm = new FormGroup(
    {
      fromAccountId: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required],
      }),
      toAccountId: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required],
      }),
      amount: new FormControl(0, {
        nonNullable: true,
        validators: [Validators.required, Validators.min(0.01)],
      }),
      description: new FormControl('', {
        nonNullable: true,
        validators: [Validators.maxLength(200)],
      }),
    },
    { validators: differentAccountsValidator },
  );

  constructor(
    private accountService: AccountService,
    private transactionService: TransactionService,
    private toastService: ToastService,
  ) {}

  ngOnInit(): void {
    this.loadAccounts();
    this.setupAccountSelectionGuards();
  }

  ngOnDestroy(): void {
    this.transferFormSubscriptions.unsubscribe();
  }

  getFromAccountOptions(): Account[] {
    const toAccountId = this.transferForm.controls.toAccountId.value;
    return this.accounts().filter((account) => account.id !== toAccountId);
  }

  getToAccountOptions(): Account[] {
    const fromAccountId = this.transferForm.controls.fromAccountId.value;
    return this.accounts().filter((account) => account.id !== fromAccountId);
  }

  get selectedFromAccount(): Account | undefined {
    const fromAccountId = this.transferForm.controls.fromAccountId.value;
    return this.accounts().find((account) => account.id === fromAccountId);
  }

  loadAccounts(): void {
    this.accountsLoading.set(true);
    this.accountsLoadError.set('');

    this.accountService.getAllAccounts().subscribe({
      next: (accounts) => {
        this.accounts.set(accounts);
        this.accountsLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.accountsLoading.set(false);
        this.accountsLoadError.set(
          this.resolveErrorMessage(error, 'Unable to load accounts.'),
        );
      },
    });
  }

  onSubmitTransfer(): void {
    this.transferFormSubmitted.set(true);
    this.transferErrorMessage.set('');

    if (this.transferForm.invalid) {
      return;
    }

    const formValues = this.transferForm.getRawValue();
    const transferRequest: TransferRequest = {
      fromAccountId: formValues.fromAccountId,
      toAccountId: formValues.toAccountId,
      amount: formValues.amount,
      description: formValues.description.trim(),
    };

    this.transferRequestInFlight.set(true);

    this.transactionService.transferFunds(transferRequest).subscribe({
      next: () => {
        this.transferRequestInFlight.set(false);
        this.toastService.showSuccess('Transfer completed successfully.', {
          label: 'View transactions',
          route: '/transactions',
        });
        this.transferFormSubmitted.set(false);
        this.transferForm.reset({
          fromAccountId: '',
          toAccountId: '',
          amount: 0,
          description: '',
        });
        this.loadAccounts();
      },
      error: (error: HttpErrorResponse) => {
        this.transferRequestInFlight.set(false);
        this.transferErrorMessage.set(
          this.resolveErrorMessage(error, 'Unable to complete transfer.'),
        );
      },
    });
  }

  hasSameAccountError(): boolean {
    return (
      this.transferFormSubmitted() &&
      this.transferForm.hasError('sameAccount')
    );
  }

  private setupAccountSelectionGuards(): void {
    const { fromAccountId, toAccountId } = this.transferForm.controls;

    this.transferFormSubscriptions.add(
      fromAccountId.valueChanges.subscribe((selectedFromId) => {
        if (selectedFromId && selectedFromId === toAccountId.value) {
          toAccountId.setValue('', { emitEvent: false });
        }
      }),
    );

    this.transferFormSubscriptions.add(
      toAccountId.valueChanges.subscribe((selectedToId) => {
        if (selectedToId && selectedToId === fromAccountId.value) {
          fromAccountId.setValue('', { emitEvent: false });
        }
      }),
    );
  }

  private resolveErrorMessage(error: HttpErrorResponse, fallbackMessage: string): string {
    const apiError = error.error as ApiError | undefined;
    if (apiError?.message) {
      return apiError.message;
    }

    return fallbackMessage;
  }
}

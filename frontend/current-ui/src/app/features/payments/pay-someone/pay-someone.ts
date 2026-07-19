import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, signal } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { AccountService } from '../../../core/services/account.service';
import { GoalService } from '../../../core/services/goal.service';
import { PaymentService } from '../../../core/services/payment.service';
import { ToastService } from '../../../core/services/toast.service';
import { Account, Goal, SendPaymentRequest } from '../../../shared/models';
import { filterNonGoalAccounts } from '../../../shared/utils/goal-account.utils';
import { resolveApiErrorMessage } from '../../../shared/utils/http-error.utils';

@Component({
  selector: 'app-pay-someone',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe],
  templateUrl: './pay-someone.html',
  styleUrl: './pay-someone.scss',
})
export class PaySomeoneComponent implements OnInit {
  accounts = signal<Account[]>([]);
  goals = signal<Goal[]>([]);
  pageLoading = signal(false);
  pageLoadError = signal('');
  paymentFormSubmitted = signal(false);
  paymentRequestInFlight = signal(false);
  paymentErrorMessage = signal('');

  userFacingAccounts = computed(() => filterNonGoalAccounts(this.accounts(), this.goals()));

  paymentForm = new FormGroup({
    fromAccountId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    recipientEmail: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    amount: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0.01)],
    }),
    reference: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(100)],
    }),
  });

  constructor(
    private accountService: AccountService,
    private goalService: GoalService,
    private paymentService: PaymentService,
    private toastService: ToastService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.loadPageData();
  }

  get selectedFromAccount(): Account | undefined {
    const fromAccountId = this.paymentForm.controls.fromAccountId.value;
    return this.userFacingAccounts().find((account) => account.id === fromAccountId);
  }

  loadPageData(): void {
    this.pageLoading.set(true);
    this.pageLoadError.set('');

    forkJoin({
      accounts: this.accountService.getAllAccounts(),
      goals: this.goalService.getAllGoals(),
    }).subscribe({
      next: (pageData) => {
        this.accounts.set(pageData.accounts);
        this.goals.set(pageData.goals);
        this.pageLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.pageLoading.set(false);
        this.pageLoadError.set(
          resolveApiErrorMessage(error, 'Unable to load payment form.'),
        );
      },
    });
  }

  onSubmitPayment(): void {
    this.paymentFormSubmitted.set(true);
    this.paymentErrorMessage.set('');

    if (this.paymentForm.invalid) {
      return;
    }

    const formValues = this.paymentForm.getRawValue();
    const sendPaymentRequest: SendPaymentRequest = {
      fromAccountId: formValues.fromAccountId,
      recipientEmail: formValues.recipientEmail.trim(),
      amount: formValues.amount,
      reference: formValues.reference.trim() || null,
    };

    this.paymentRequestInFlight.set(true);
    const idempotencyKey = crypto.randomUUID();

    this.paymentService.sendPayment(sendPaymentRequest, idempotencyKey).subscribe({
      next: (receipt) => {
        this.paymentRequestInFlight.set(false);
        this.toastService.showSuccess('Payment sent successfully.', {
          label: 'View receipt',
          route: `/payments/${receipt.transactionId}`,
        });
        void this.router.navigate(['/payments', receipt.transactionId]);
      },
      error: (error: HttpErrorResponse) => {
        this.paymentRequestInFlight.set(false);
        this.paymentErrorMessage.set(
          resolveApiErrorMessage(error, 'Unable to send payment.'),
        );
      },
    });
  }
}

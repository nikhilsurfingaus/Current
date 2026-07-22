import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin } from 'rxjs';

import { AccountService } from '../../../core/services/account.service';
import { GoalService } from '../../../core/services/goal.service';
import { LoanService } from '../../../core/services/loan.service';
import { ToastService } from '../../../core/services/toast.service';
import { NormalizeAmountDirective } from '../../../shared/directives/normalize-amount.directive';
import { Account, Loan, LoanRepayment, LoanStatus, RepayLoanRequest } from '../../../shared/models';
import { filterNonGoalAccounts } from '../../../shared/utils/goal-account.utils';
import { focusFirstInvalidControl } from '../../../shared/utils/form-accessibility.utils';
import { resolveApiErrorMessage } from '../../../shared/utils/http-error.utils';
import {
  getLoanRepaymentProgressPercent,
  getLoanStatusLabel,
  isLoanCancellable,
  isLoanRepayable,
} from '../../../shared/utils/loan-status.utils';

@Component({
  selector: 'app-loan-detail',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe, DatePipe, NormalizeAmountDirective],
  templateUrl: './loan-detail.html',
  styleUrl: './loan-detail.scss',
})
export class LoanDetailComponent implements OnInit {
  loan = signal<Loan | null>(null);
  repaymentHistory = signal<LoanRepayment[]>([]);
  sourceAccounts = signal<Account[]>([]);
  pageLoading = signal(false);
  pageLoadError = signal('');
  historyLoading = signal(false);

  repayPanelOpen = signal(false);
  repayFormSubmitted = signal(false);
  repayRequestInFlight = signal(false);
  cancelRequestInFlight = signal(false);
  actionErrorMessage = signal('');

  readonly loanStatus = LoanStatus;
  readonly getLoanStatusLabel = getLoanStatusLabel;
  readonly getLoanRepaymentProgressPercent = getLoanRepaymentProgressPercent;
  readonly isLoanRepayable = isLoanRepayable;
  readonly isLoanCancellable = isLoanCancellable;

  repayForm = new FormGroup({
    amount: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0.01)],
    }),
    sourceAccountId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  repaymentProgressPercent = computed(() => {
    const currentLoan = this.loan();
    return currentLoan ? getLoanRepaymentProgressPercent(currentLoan) : 0;
  });

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private loanService: LoanService,
    private accountService: AccountService,
    private goalService: GoalService,
    private toastService: ToastService,
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const loanId = params.get('id');

      if (!loanId) {
        void this.router.navigate(['/loans']);
        return;
      }

      this.loadLoanDetail(loanId);
    });
  }

  loadLoanDetail(loanId: string): void {
    this.pageLoading.set(true);
    this.pageLoadError.set('');

    this.loanService.getLoanById(loanId).subscribe({
      next: (loanData) => {
        this.loan.set(loanData);
        this.loadSourceAccounts();
        this.loadRepaymentHistory(loanId);
        this.pageLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.pageLoading.set(false);
        this.pageLoadError.set(resolveApiErrorMessage(error, 'Unable to load loan.'));
      },
    });
  }

  openRepayPanel(): void {
    const currentLoan = this.loan();
    if (!currentLoan) {
      return;
    }

    this.repayPanelOpen.set(true);
    this.repayFormSubmitted.set(false);
    this.actionErrorMessage.set('');
    this.repayForm.reset({
      amount: Math.min(currentLoan.monthlyPayment, currentLoan.outstandingPrincipal),
      sourceAccountId: this.sourceAccounts()[0]?.id ?? '',
    });
  }

  closeRepayPanel(): void {
    this.repayPanelOpen.set(false);
    this.repayFormSubmitted.set(false);
    this.actionErrorMessage.set('');
  }

  onSubmitRepayment(): void {
    const currentLoan = this.loan();
    if (!currentLoan) {
      return;
    }

    this.repayFormSubmitted.set(true);
    this.actionErrorMessage.set('');

    if (this.repayForm.invalid) {
      focusFirstInvalidControl(this.repayForm);
      return;
    }

    const formValues = this.repayForm.getRawValue();
    const repayLoanRequest: RepayLoanRequest = {
      amount: formValues.amount,
      sourceAccountId: formValues.sourceAccountId,
    };

    this.repayRequestInFlight.set(true);

    this.loanService.repayLoan(currentLoan.id, repayLoanRequest).subscribe({
      next: (updatedLoan) => {
        this.repayRequestInFlight.set(false);
        this.loan.set(updatedLoan);
        this.loadRepaymentHistory(currentLoan.id);
        this.closeRepayPanel();
        this.toastService.showSuccess('Repayment completed.');
      },
      error: (error: HttpErrorResponse) => {
        this.repayRequestInFlight.set(false);
        this.actionErrorMessage.set(resolveApiErrorMessage(error, 'Unable to process repayment.'));
      },
    });
  }

  onCancelLoanRequest(): void {
    const currentLoan = this.loan();
    if (!currentLoan) {
      return;
    }

    this.cancelRequestInFlight.set(true);
    this.actionErrorMessage.set('');

    this.loanService.cancelLoanRequest(currentLoan.id).subscribe({
      next: (updatedLoan) => {
        this.cancelRequestInFlight.set(false);
        this.loan.set(updatedLoan);
        this.toastService.showSuccess('Loan request cancelled.');
      },
      error: (error: HttpErrorResponse) => {
        this.cancelRequestInFlight.set(false);
        this.actionErrorMessage.set(resolveApiErrorMessage(error, 'Unable to cancel loan request.'));
      },
    });
  }

  private loadSourceAccounts(): void {
    forkJoin({
      accounts: this.accountService.getAllAccounts(),
      goals: this.goalService.getAllGoals(),
    }).subscribe({
      next: ({ accounts, goals }) => {
        this.sourceAccounts.set(filterNonGoalAccounts(accounts, goals));
      },
    });
  }

  private loadRepaymentHistory(loanId: string): void {
    this.historyLoading.set(true);

    this.loanService.getRepaymentHistory(loanId).subscribe({
      next: (repayments) => {
        this.repaymentHistory.set(repayments);
        this.historyLoading.set(false);
      },
      error: () => {
        this.historyLoading.set(false);
      },
    });
  }
}

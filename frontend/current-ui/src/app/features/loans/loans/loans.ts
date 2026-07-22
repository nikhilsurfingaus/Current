import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { AccountService } from '../../../core/services/account.service';
import { LoanService } from '../../../core/services/loan.service';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state';
import { SkeletonLoaderComponent } from '../../../shared/components/skeleton-loader/skeleton-loader';
import { NormalizeAmountDirective } from '../../../shared/directives/normalize-amount.directive';
import { Account, CreateLoanRequest, Loan, LoanStatus } from '../../../shared/models';
import { filterNonGoalAccounts } from '../../../shared/utils/goal-account.utils';
import { focusFirstInvalidControl } from '../../../shared/utils/form-accessibility.utils';
import { resolveApiErrorMessage } from '../../../shared/utils/http-error.utils';
import {
  LOAN_STATUS_FILTER_OPTIONS,
  getLoanRepaymentProgressPercent,
  getLoanStatusLabel,
} from '../../../shared/utils/loan-status.utils';

@Component({
  selector: 'app-loans',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    CurrencyPipe,
    DatePipe,
    NormalizeAmountDirective,
    SkeletonLoaderComponent,
    EmptyStateComponent,
  ],
  templateUrl: './loans.html',
  styleUrl: './loans.scss',
})
export class LoansComponent implements OnInit {
  loans = signal<Loan[]>([]);
  fundingAccounts = signal<Account[]>([]);
  loansLoading = signal(false);
  loansLoadError = signal('');
  requestPanelOpen = signal(false);
  requestFormSubmitted = signal(false);
  requestInFlight = signal(false);
  requestErrorMessage = signal('');
  statusFilter = signal<LoanStatus | null>(null);

  readonly statusFilterOptions = LOAN_STATUS_FILTER_OPTIONS;
  readonly loanStatus = LoanStatus;
  readonly getLoanStatusLabel = getLoanStatusLabel;
  readonly getLoanRepaymentProgressPercent = getLoanRepaymentProgressPercent;

  filteredLoans = computed(() => {
    const selectedStatus = this.statusFilter();

    if (selectedStatus === null) {
      return this.loans();
    }

    return this.loans().filter((loan) => loan.status === selectedStatus);
  });

  activeLoansCount = computed(
    () => this.loans().filter((loan) => loan.status === LoanStatus.Active || loan.status === LoanStatus.Overdue).length,
  );

  pendingLoansCount = computed(
    () => this.loans().filter((loan) => loan.status === LoanStatus.Pending).length,
  );

  totalOutstanding = computed(() =>
    this.loans()
      .filter((loan) => loan.status === LoanStatus.Active || loan.status === LoanStatus.Overdue)
      .reduce((total, loan) => total + loan.outstandingPrincipal, 0),
  );

  summaryCurrency = computed(() => this.loans().find((loan) => loan.currency)?.currency ?? 'AUD');

  requestLoanForm = new FormGroup({
    principal: new FormControl(5000, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(500)],
    }),
    termMonths: new FormControl(12, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(1), Validators.max(60)],
    }),
    fundedAccountId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    purpose: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(500)],
    }),
  });

  constructor(
    private loanService: LoanService,
    private accountService: AccountService,
  ) {}

  ngOnInit(): void {
    this.loadLoansPageData();
  }

  loadLoansPageData(): void {
    this.loansLoading.set(true);
    this.loansLoadError.set('');

    this.loanService.getAllLoans().subscribe({
      next: (loans) => {
        this.loans.set(loans);
        this.loadFundingAccounts();
      },
      error: (error: HttpErrorResponse) => {
        this.loansLoading.set(false);
        this.loansLoadError.set(resolveApiErrorMessage(error, 'Unable to load loans.'));
      },
    });
  }

  setStatusFilter(status: LoanStatus | null): void {
    this.statusFilter.set(status);
  }

  isStatusFilterActive(status: LoanStatus | null): boolean {
    return this.statusFilter() === status;
  }

  openRequestPanel(): void {
    this.requestPanelOpen.set(true);
    this.requestFormSubmitted.set(false);
    this.requestErrorMessage.set('');
    this.requestLoanForm.reset({
      principal: 5000,
      termMonths: 12,
      fundedAccountId: this.fundingAccounts()[0]?.id ?? '',
      purpose: '',
    });
  }

  closeRequestPanel(): void {
    this.requestPanelOpen.set(false);
    this.requestFormSubmitted.set(false);
    this.requestErrorMessage.set('');
  }

  onSubmitLoanRequest(): void {
    this.requestFormSubmitted.set(true);
    this.requestErrorMessage.set('');

    if (this.requestLoanForm.invalid) {
      focusFirstInvalidControl(this.requestLoanForm);
      return;
    }

    const formValues = this.requestLoanForm.getRawValue();
    const createLoanRequest: CreateLoanRequest = {
      principal: formValues.principal,
      termMonths: formValues.termMonths,
      fundedAccountId: formValues.fundedAccountId,
    };

    const purpose = formValues.purpose.trim();
    if (purpose) {
      createLoanRequest.purpose = purpose;
    }

    this.requestInFlight.set(true);

    this.loanService.createLoanRequest(createLoanRequest).subscribe({
      next: (createdLoan) => {
        this.requestInFlight.set(false);
        this.loans.set([createdLoan, ...this.loans()]);
        this.closeRequestPanel();
      },
      error: (error: HttpErrorResponse) => {
        this.requestInFlight.set(false);
        this.requestErrorMessage.set(resolveApiErrorMessage(error, 'Unable to submit loan request.'));
      },
    });
  }

  private loadFundingAccounts(): void {
    this.accountService.getAllAccounts().subscribe({
      next: (accounts) => {
        this.fundingAccounts.set(filterNonGoalAccounts(accounts, []));
        this.loansLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loansLoading.set(false);
        this.loansLoadError.set(resolveApiErrorMessage(error, 'Unable to load accounts.'));
      },
    });
  }
}

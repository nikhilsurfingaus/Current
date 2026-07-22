import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { BranchService } from '../../../core/services/branch.service';
import { ToastService } from '../../../core/services/toast.service';
import { SkeletonLoaderComponent } from '../../../shared/components/skeleton-loader/skeleton-loader';
import { BranchTreasury, CreateBranchDisbursementRequest, LoanAdmin, LoanStatus } from '../../../shared/models';
import { focusFirstInvalidControl } from '../../../shared/utils/form-accessibility.utils';
import { resolveApiErrorMessage } from '../../../shared/utils/http-error.utils';
import { getLoanStatusLabel } from '../../../shared/utils/loan-status.utils';

@Component({
  selector: 'app-branch-admin',
  standalone: true,
  imports: [ReactiveFormsModule, CurrencyPipe, DatePipe, SkeletonLoaderComponent],
  templateUrl: './branch-admin.html',
  styleUrl: './branch-admin.scss',
})
export class BranchAdminComponent implements OnInit {
  treasuryLoading = signal(true);
  treasuryLoadError = signal('');
  treasury = signal<BranchTreasury | null>(null);
  disbursementSubmitted = signal(false);
  disbursementInFlight = signal(false);
  disbursementError = signal('');

  pendingLoansLoading = signal(false);
  pendingLoansLoadError = signal('');
  pendingLoans = signal<LoanAdmin[]>([]);
  loanActionInFlightId = signal<string | null>(null);
  rejectPanelLoanId = signal<string | null>(null);
  rejectFormSubmitted = signal(false);
  rejectError = signal('');

  readonly getLoanStatusLabel = getLoanStatusLabel;
  readonly loanStatus = LoanStatus;

  disbursementForm = new FormGroup({
    recipientEmail: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    amount: new FormControl(2500, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0.01)],
    }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(200)],
    }),
  });

  rejectLoanForm = new FormGroup({
    reason: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(500)],
    }),
  });

  constructor(
    private branchService: BranchService,
    private toastService: ToastService,
  ) {}

  ngOnInit(): void {
    this.loadTreasury();
    this.loadPendingLoans();
  }

  loadTreasury(): void {
    this.treasuryLoading.set(true);
    this.treasuryLoadError.set('');

    this.branchService.getTreasury().subscribe({
      next: (treasuryData) => {
        this.treasury.set(treasuryData);
        this.treasuryLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.treasuryLoading.set(false);
        this.treasuryLoadError.set(
          resolveApiErrorMessage(error, 'Unable to load branch treasury.'),
        );
      },
    });
  }

  onSubmitDisbursement(): void {
    this.disbursementSubmitted.set(true);
    this.disbursementError.set('');

    if (this.disbursementForm.invalid) {
      focusFirstInvalidControl(this.disbursementForm);
      return;
    }

    const formValues = this.disbursementForm.getRawValue();
    const request: CreateBranchDisbursementRequest = {
      recipientEmail: formValues.recipientEmail.trim().toLowerCase(),
      amount: formValues.amount,
    };

    const description = formValues.description.trim();
    if (description) {
      request.description = description;
    }

    this.disbursementInFlight.set(true);

    this.branchService.createDisbursement(request).subscribe({
      next: (disbursement) => {
        this.disbursementInFlight.set(false);
        this.disbursementSubmitted.set(false);
        this.disbursementForm.patchValue({ amount: 2500, description: '' });
        this.loadTreasury();
        this.toastService.showSuccess(
          `Sent ${disbursement.amount.toLocaleString('en-AU', {
            style: 'currency',
            currency: disbursement.currency,
          })} to ${disbursement.recipientName}.`,
        );
      },
      error: (error: HttpErrorResponse) => {
        this.disbursementInFlight.set(false);
        this.disbursementError.set(
          resolveApiErrorMessage(error, 'Unable to create disbursement.'),
        );
      },
    });
  }

  loadPendingLoans(): void {
    this.pendingLoansLoading.set(true);
    this.pendingLoansLoadError.set('');

    this.branchService.getLoans(LoanStatus.Pending).subscribe({
      next: (loans) => {
        this.pendingLoans.set(loans);
        this.pendingLoansLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.pendingLoansLoading.set(false);
        this.pendingLoansLoadError.set(
          resolveApiErrorMessage(error, 'Unable to load pending loans.'),
        );
      },
    });
  }

  onApproveLoan(loanId: string): void {
    this.loanActionInFlightId.set(loanId);

    this.branchService.approveLoan(loanId).subscribe({
      next: () => {
        this.loanActionInFlightId.set(null);
        this.loadTreasury();
        this.loadPendingLoans();
        this.toastService.showSuccess('Loan approved and disbursed.');
      },
      error: (error: HttpErrorResponse) => {
        this.loanActionInFlightId.set(null);
        this.toastService.showError(resolveApiErrorMessage(error, 'Unable to approve loan.'));
      },
    });
  }

  openRejectPanel(loanId: string): void {
    this.rejectPanelLoanId.set(loanId);
    this.rejectFormSubmitted.set(false);
    this.rejectError.set('');
    this.rejectLoanForm.reset({ reason: '' });
  }

  closeRejectPanel(): void {
    this.rejectPanelLoanId.set(null);
    this.rejectFormSubmitted.set(false);
    this.rejectError.set('');
  }

  onSubmitRejectLoan(): void {
    const loanId = this.rejectPanelLoanId();
    if (!loanId) {
      return;
    }

    this.rejectFormSubmitted.set(true);
    this.rejectError.set('');

    if (this.rejectLoanForm.invalid) {
      focusFirstInvalidControl(this.rejectLoanForm);
      return;
    }

    this.loanActionInFlightId.set(loanId);

    this.branchService.rejectLoan(loanId, {
      reason: this.rejectLoanForm.controls.reason.value.trim(),
    }).subscribe({
      next: () => {
        this.loanActionInFlightId.set(null);
        this.closeRejectPanel();
        this.loadPendingLoans();
        this.toastService.showSuccess('Loan request rejected.');
      },
      error: (error: HttpErrorResponse) => {
        this.loanActionInFlightId.set(null);
        this.rejectError.set(resolveApiErrorMessage(error, 'Unable to reject loan.'));
      },
    });
  }
}

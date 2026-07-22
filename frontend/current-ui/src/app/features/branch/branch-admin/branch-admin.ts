import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { BranchService } from '../../../core/services/branch.service';
import { ToastService } from '../../../core/services/toast.service';
import { SkeletonLoaderComponent } from '../../../shared/components/skeleton-loader/skeleton-loader';
import { BranchTreasury, CreateBranchDisbursementRequest } from '../../../shared/models';
import { focusFirstInvalidControl } from '../../../shared/utils/form-accessibility.utils';
import { resolveApiErrorMessage } from '../../../shared/utils/http-error.utils';

@Component({
  selector: 'app-branch-admin',
  standalone: true,
  imports: [ReactiveFormsModule, CurrencyPipe, SkeletonLoaderComponent],
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

  constructor(
    private branchService: BranchService,
    private toastService: ToastService,
  ) {}

  ngOnInit(): void {
    this.loadTreasury();
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
}

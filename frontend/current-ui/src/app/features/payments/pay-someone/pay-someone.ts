import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, signal } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { AccountService } from '../../../core/services/account.service';
import { ContactService } from '../../../core/services/contact.service';
import { GoalService } from '../../../core/services/goal.service';
import { PaymentService } from '../../../core/services/payment.service';
import { ToastService } from '../../../core/services/toast.service';
import {
  Account,
  Contact,
  CreateContactRequest,
  Goal,
  PaymentReceipt,
  SendPaymentRequest,
} from '../../../shared/models';
import { NormalizeAmountDirective } from '../../../shared/directives/normalize-amount.directive';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state';
import { SkeletonLoaderComponent } from '../../../shared/components/skeleton-loader/skeleton-loader';
import { formatBankAccountLine } from '../../../shared/utils/bank-account.utils';
import { filterNonGoalAccounts } from '../../../shared/utils/goal-account.utils';
import { focusFirstInvalidControl } from '../../../shared/utils/form-accessibility.utils';
import { resolveApiErrorMessage } from '../../../shared/utils/http-error.utils';

type PaymentMethod = 'email' | 'bsb';

const BSB_PATTERN = /^\d{3}-?\d{3}$/;
const ACCOUNT_NUMBER_PATTERN = /^\d{6,9}$/;

@Component({
  selector: 'app-pay-someone',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe, NormalizeAmountDirective, SkeletonLoaderComponent, EmptyStateComponent],
  templateUrl: './pay-someone.html',
  styleUrl: './pay-someone.scss',
})
export class PaySomeoneComponent implements OnInit {
  accounts = signal<Account[]>([]);
  goals = signal<Goal[]>([]);
  contacts = signal<Contact[]>([]);
  pageLoading = signal(false);
  pageLoadError = signal('');
  paymentFormSubmitted = signal(false);
  paymentRequestInFlight = signal(false);
  paymentErrorMessage = signal('');

  userFacingAccounts = computed(() => filterNonGoalAccounts(this.accounts(), this.goals()));

  paymentForm = new FormGroup({
    selectedContactId: new FormControl('', {
      nonNullable: true,
    }),
    paymentMethod: new FormControl<PaymentMethod>('email', {
      nonNullable: true,
    }),
    fromAccountId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    recipientEmail: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    recipientBsb: new FormControl('', {
      nonNullable: true,
    }),
    recipientAccountNumber: new FormControl('', {
      nonNullable: true,
    }),
    amount: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0.01)],
    }),
    reference: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(100)],
    }),
    saveContact: new FormControl(false, {
      nonNullable: true,
    }),
    contactName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(100)],
    }),
  });

  constructor(
    private accountService: AccountService,
    private contactService: ContactService,
    private goalService: GoalService,
    private paymentService: PaymentService,
    private toastService: ToastService,
    private activatedRoute: ActivatedRoute,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.applyPaymentMethodValidators(this.paymentForm.controls.paymentMethod.value);
    this.loadPageData();
  }

  get selectedFromAccount(): Account | undefined {
    const fromAccountId = this.paymentForm.controls.fromAccountId.value;
    return this.userFacingAccounts().find((account) => account.id === fromAccountId);
  }

  get recipientAlreadySaved(): boolean {
    const formValues = this.paymentForm.getRawValue();

    return this.contacts().some((contact) => {
      if (formValues.paymentMethod === 'email') {
        const recipientEmail = formValues.recipientEmail.trim().toLowerCase();
        return contact.email?.toLowerCase() === recipientEmail;
      }

      const recipientBsb = this.normalizeBsbInput(formValues.recipientBsb);
      const recipientAccountNumber = formValues.recipientAccountNumber.trim();
      return (
        contact.bsb === recipientBsb && contact.accountNumber === recipientAccountNumber
      );
    });
  }

  formatContactLabel(contact: Contact): string {
    if (contact.email) {
      return `${contact.name} (${contact.email})`;
    }

    return contact.name;
  }

  onPaymentMethodChanged(): void {
    this.applyPaymentMethodValidators(this.paymentForm.controls.paymentMethod.value);
    this.paymentForm.patchValue({
      selectedContactId: '',
      recipientEmail: '',
      recipientBsb: '',
      recipientAccountNumber: '',
      saveContact: false,
      contactName: '',
    });
  }

  onSavedContactSelected(): void {
    const selectedContactId = this.paymentForm.controls.selectedContactId.value;
    const selectedContact = this.contacts().find((contact) => contact.id === selectedContactId);

    if (!selectedContact) {
      return;
    }

    if (selectedContact.bsb && selectedContact.accountNumber) {
      this.paymentForm.patchValue({
        paymentMethod: 'bsb',
        recipientEmail: '',
        recipientBsb: selectedContact.bsb,
        recipientAccountNumber: selectedContact.accountNumber,
        contactName: selectedContact.name,
        saveContact: false,
      });
      this.applyPaymentMethodValidators('bsb');
      return;
    }

    this.paymentForm.patchValue({
      paymentMethod: 'email',
      recipientEmail: selectedContact.email ?? '',
      recipientBsb: '',
      recipientAccountNumber: '',
      contactName: selectedContact.name,
      saveContact: false,
    });
    this.applyPaymentMethodValidators('email');
  }

  loadPageData(): void {
    this.pageLoading.set(true);
    this.pageLoadError.set('');

    forkJoin({
      accounts: this.accountService.getAllAccounts(),
      contacts: this.contactService.getAllContacts(),
      goals: this.goalService.getAllGoals(),
    }).subscribe({
      next: (pageData) => {
        this.accounts.set(pageData.accounts);
        this.contacts.set(pageData.contacts);
        this.goals.set(pageData.goals);
        this.selectContactFromQuery(pageData.contacts);
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
      focusFirstInvalidControl(this.paymentForm);
      return;
    }

    const formValues = this.paymentForm.getRawValue();

    if (formValues.saveContact && !this.recipientAlreadySaved && !formValues.contactName.trim()) {
      this.paymentErrorMessage.set('Enter a name to save this recipient as a contact.');
      return;
    }

    const sendPaymentRequest = this.buildSendPaymentRequest(formValues);
    this.paymentRequestInFlight.set(true);
    const idempotencyKey = crypto.randomUUID();

    this.paymentService.sendPayment(sendPaymentRequest, idempotencyKey).subscribe({
      next: (receipt) => {
        this.paymentRequestInFlight.set(false);
        this.saveContactAfterPayment(receipt);
      },
      error: (error: HttpErrorResponse) => {
        this.paymentRequestInFlight.set(false);
        this.paymentErrorMessage.set(
          resolveApiErrorMessage(error, 'Unable to send payment.'),
        );
      },
    });
  }

  private buildSendPaymentRequest(formValues: ReturnType<typeof this.paymentForm.getRawValue>): SendPaymentRequest {
    if (formValues.paymentMethod === 'bsb') {
      return {
        fromAccountId: formValues.fromAccountId,
        recipientBsb: this.normalizeBsbInput(formValues.recipientBsb),
        recipientAccountNumber: formValues.recipientAccountNumber.trim(),
        amount: formValues.amount,
        reference: formValues.reference.trim() || null,
      };
    }

    return {
      fromAccountId: formValues.fromAccountId,
      recipientEmail: formValues.recipientEmail.trim(),
      amount: formValues.amount,
      reference: formValues.reference.trim() || null,
    };
  }

  private selectContactFromQuery(contacts: Contact[]): void {
    const contactId = this.activatedRoute.snapshot.queryParamMap.get('contactId');
    const selectedContact = contacts.find((contact) => contact.id === contactId);

    if (!selectedContact) {
      return;
    }

    this.paymentForm.patchValue({
      selectedContactId: selectedContact.id,
    });
    this.onSavedContactSelected();
  }

  private saveContactAfterPayment(receipt: PaymentReceipt): void {
    const formValues = this.paymentForm.getRawValue();

    if (!formValues.saveContact || this.recipientAlreadySaved) {
      this.completePaymentSuccess(receipt);
      return;
    }

    const createContactRequest: CreateContactRequest =
      formValues.paymentMethod === 'bsb'
        ? {
            name: formValues.contactName.trim(),
            bsb: this.normalizeBsbInput(formValues.recipientBsb),
            accountNumber: formValues.recipientAccountNumber.trim(),
          }
        : {
            name: formValues.contactName.trim(),
            email: formValues.recipientEmail.trim(),
          };

    this.contactService.createContact(createContactRequest).subscribe({
      next: () => {
        this.toastService.showSuccess('Payment sent and contact saved.');
        void this.router.navigate(['/payments', receipt.transactionId]);
      },
      error: (error: HttpErrorResponse) => {
        this.toastService.showError(
          resolveApiErrorMessage(error, 'Payment sent, but the contact could not be saved.'),
        );
        void this.router.navigate(['/payments', receipt.transactionId]);
      },
    });
  }

  private completePaymentSuccess(receipt: PaymentReceipt): void {
    this.toastService.showSuccess('Payment sent successfully.', {
      label: 'View receipt',
      route: `/payments/${receipt.transactionId}`,
    });
    void this.router.navigate(['/payments', receipt.transactionId]);
  }

  private applyPaymentMethodValidators(paymentMethod: PaymentMethod): void {
    const emailControl = this.paymentForm.controls.recipientEmail;
    const bsbControl = this.paymentForm.controls.recipientBsb;
    const accountNumberControl = this.paymentForm.controls.recipientAccountNumber;

    if (paymentMethod === 'email') {
      emailControl.setValidators([Validators.required, Validators.email]);
      bsbControl.clearValidators();
      accountNumberControl.clearValidators();
    } else {
      emailControl.clearValidators();
      bsbControl.setValidators([Validators.required, Validators.pattern(BSB_PATTERN)]);
      accountNumberControl.setValidators([
        Validators.required,
        Validators.pattern(ACCOUNT_NUMBER_PATTERN),
      ]);
    }

    emailControl.updateValueAndValidity();
    bsbControl.updateValueAndValidity();
    accountNumberControl.updateValueAndValidity();
  }

  private normalizeBsbInput(bsbValue: string): string {
    const digitsOnly = bsbValue.replace(/\D/g, '');

    if (digitsOnly.length !== 6) {
      return bsbValue.trim();
    }

    return `${digitsOnly.slice(0, 3)}-${digitsOnly.slice(3)}`;
  }
}

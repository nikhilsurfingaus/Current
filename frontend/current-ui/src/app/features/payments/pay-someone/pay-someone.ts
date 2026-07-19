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
import { filterNonGoalAccounts } from '../../../shared/utils/goal-account.utils';
import { resolveApiErrorMessage } from '../../../shared/utils/http-error.utils';

@Component({
  selector: 'app-pay-someone',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe, NormalizeAmountDirective],
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
    this.loadPageData();
  }

  get selectedFromAccount(): Account | undefined {
    const fromAccountId = this.paymentForm.controls.fromAccountId.value;
    return this.userFacingAccounts().find((account) => account.id === fromAccountId);
  }

  get recipientAlreadySaved(): boolean {
    const recipientEmail = this.paymentForm.controls.recipientEmail.value.trim().toLowerCase();
    return this.contacts().some((contact) => contact.email.toLowerCase() === recipientEmail);
  }

  onSavedContactSelected(): void {
    const selectedContactId = this.paymentForm.controls.selectedContactId.value;
    const selectedContact = this.contacts().find((contact) => contact.id === selectedContactId);

    if (!selectedContact) {
      return;
    }

    this.paymentForm.patchValue({
      recipientEmail: selectedContact.email,
      contactName: selectedContact.name,
      saveContact: false,
    });
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
      return;
    }

    const formValues = this.paymentForm.getRawValue();

    if (formValues.saveContact && !this.recipientAlreadySaved && !formValues.contactName.trim()) {
      this.paymentErrorMessage.set('Enter a name to save this recipient as a contact.');
      return;
    }

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

  private selectContactFromQuery(contacts: Contact[]): void {
    const contactId = this.activatedRoute.snapshot.queryParamMap.get('contactId');
    const selectedContact = contacts.find((contact) => contact.id === contactId);

    if (!selectedContact) {
      return;
    }

    this.paymentForm.patchValue({
      selectedContactId: selectedContact.id,
      recipientEmail: selectedContact.email,
      contactName: selectedContact.name,
    });
  }

  private saveContactAfterPayment(receipt: PaymentReceipt): void {
    const formValues = this.paymentForm.getRawValue();

    if (!formValues.saveContact || this.recipientAlreadySaved) {
      this.completePaymentSuccess(receipt);
      return;
    }

    const createContactRequest: CreateContactRequest = {
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
}

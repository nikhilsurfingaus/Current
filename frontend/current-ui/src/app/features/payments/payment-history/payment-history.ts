import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { ContactService } from '../../../core/services/contact.service';
import { PaymentService } from '../../../core/services/payment.service';
import { ToastService } from '../../../core/services/toast.service';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state';
import { SkeletonLoaderComponent } from '../../../shared/components/skeleton-loader/skeleton-loader';
import { PaymentDirection, PaymentHistoryItem, TransactionStatus } from '../../../shared/models';
import { formatBankAccountLine } from '../../../shared/utils/bank-account.utils';
import { resolveApiErrorMessage } from '../../../shared/utils/http-error.utils';
import { getTransactionStatusLabel } from '../../../shared/utils/transaction-status.utils';

type PaymentHistoryFilter = 'all' | 'sent' | 'received';

@Component({
  selector: 'app-payment-history',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink, SkeletonLoaderComponent, EmptyStateComponent],
  templateUrl: './payment-history.html',
  styleUrl: './payment-history.scss',
})
export class PaymentHistoryComponent implements OnInit {
  payments = signal<PaymentHistoryItem[]>([]);
  paymentsLoading = signal(false);
  paymentsLoadError = signal('');
  activeFilter = signal<PaymentHistoryFilter>('all');
  contactsLoaded = signal(false);
  savedContactEmails = signal<Set<string>>(new Set());
  addingContactEmail = signal<string | null>(null);

  readonly paymentDirection = PaymentDirection;
  readonly getTransactionStatusLabel = getTransactionStatusLabel;
  readonly transactionStatus = TransactionStatus;

  filteredPayments = computed(() => {
    const filter = this.activeFilter();
    const payments = this.payments();

    if (filter === 'sent') {
      return payments.filter((payment) => payment.direction === PaymentDirection.Sent);
    }

    if (filter === 'received') {
      return payments.filter((payment) => payment.direction === PaymentDirection.Received);
    }

    return payments;
  });

  constructor(
    private paymentService: PaymentService,
    private contactService: ContactService,
    private toastService: ToastService,
  ) {}

  ngOnInit(): void {
    this.loadPaymentHistory();
    this.loadContacts();
  }

  setFilter(filter: PaymentHistoryFilter): void {
    this.activeFilter.set(filter);
  }

  getCounterpartyLabel(payment: PaymentHistoryItem): string {
    if (payment.direction === PaymentDirection.Sent) {
      return (
        payment.recipientName ||
        payment.recipientEmail ||
        formatBankAccountLine(payment.recipientBsb, payment.recipientAccountNumber)
      );
    }

    return payment.senderName || payment.senderEmail;
  }

  getCounterpartyEmail(payment: PaymentHistoryItem): string {
    return payment.direction === PaymentDirection.Sent
      ? payment.recipientEmail ?? ''
      : payment.senderEmail;
  }

  isCounterpartySaved(payment: PaymentHistoryItem): boolean {
    return this.savedContactEmails().has(this.getCounterpartyEmail(payment).toLowerCase());
  }

  addCounterpartyToContacts(payment: PaymentHistoryItem): void {
    const contactEmail = this.getCounterpartyEmail(payment).trim().toLowerCase();

    if (!contactEmail || this.savedContactEmails().has(contactEmail)) {
      return;
    }

    this.addingContactEmail.set(contactEmail);
    this.contactService.createContact({
      name: this.getCounterpartyLabel(payment),
      email: contactEmail,
    }).subscribe({
      next: () => {
        this.savedContactEmails.update((contactEmails) => new Set(contactEmails).add(contactEmail));
        this.addingContactEmail.set(null);
        this.toastService.showSuccess('Contact saved.');
      },
      error: (error: HttpErrorResponse) => {
        this.addingContactEmail.set(null);
        this.toastService.showError(resolveApiErrorMessage(error, 'Unable to save contact.'));
      },
    });
  }

  loadPaymentHistory(): void {
    this.paymentsLoading.set(true);
    this.paymentsLoadError.set('');

    this.paymentService.getPaymentHistory().subscribe({
      next: (payments) => {
        this.payments.set(payments);
        this.paymentsLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.paymentsLoading.set(false);
        this.paymentsLoadError.set(
          resolveApiErrorMessage(error, 'Unable to load payment history.'),
        );
      },
    });
  }

  private loadContacts(): void {
    this.contactService.getAllContacts().subscribe({
      next: (contacts) => {
        this.savedContactEmails.set(
          new Set(
            contacts
              .flatMap((contact) => (contact.email ? [contact.email.trim().toLowerCase()] : [])),
          ),
        );
        this.contactsLoaded.set(true);
      },
    });
  }
}

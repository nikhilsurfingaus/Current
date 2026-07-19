import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { PaymentService } from '../../../core/services/payment.service';
import { PaymentDirection, PaymentHistoryItem, TransactionStatus } from '../../../shared/models';
import { resolveApiErrorMessage } from '../../../shared/utils/http-error.utils';
import { getTransactionStatusLabel } from '../../../shared/utils/transaction-status.utils';

type PaymentHistoryFilter = 'all' | 'sent' | 'received';

@Component({
  selector: 'app-payment-history',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './payment-history.html',
  styleUrl: './payment-history.scss',
})
export class PaymentHistoryComponent implements OnInit {
  payments = signal<PaymentHistoryItem[]>([]);
  paymentsLoading = signal(false);
  paymentsLoadError = signal('');
  activeFilter = signal<PaymentHistoryFilter>('all');

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

  constructor(private paymentService: PaymentService) {}

  ngOnInit(): void {
    this.loadPaymentHistory();
  }

  setFilter(filter: PaymentHistoryFilter): void {
    this.activeFilter.set(filter);
  }

  getCounterpartyLabel(payment: PaymentHistoryItem): string {
    if (payment.direction === PaymentDirection.Sent) {
      return payment.recipientName || payment.recipientEmail;
    }

    return payment.senderName || payment.senderEmail;
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
}

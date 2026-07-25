import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { PaymentService } from '../../../core/services/payment.service';
import { PaymentDirection, PaymentHistoryItem, TransactionStatus } from '../../../shared/models';
import { formatBankAccountLine } from '../../../shared/utils/bank-account.utils';
import { resolveApiErrorMessage } from '../../../shared/utils/http-error.utils';
import { getTransactionStatusLabel } from '../../../shared/utils/transaction-status.utils';

@Component({
  selector: 'app-payment-receipt',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './payment-receipt.html',
  styleUrl: './payment-receipt.scss',
})
export class PaymentReceiptComponent implements OnInit {
  receipt = signal<PaymentHistoryItem | null>(null);
  receiptLoading = signal(false);
  receiptLoadError = signal('');

  readonly paymentDirection = PaymentDirection;
  readonly getTransactionStatusLabel = getTransactionStatusLabel;
  readonly transactionStatus = TransactionStatus;
  readonly formatBankAccountLine = formatBankAccountLine;

  getRecipientDetailsLine(receipt: PaymentHistoryItem): string {
    if (receipt.recipientEmail) {
      return receipt.recipientEmail;
    }

    return formatBankAccountLine(receipt.recipientBsb, receipt.recipientAccountNumber);
  }

  constructor(
    private activatedRoute: ActivatedRoute,
    private paymentService: PaymentService,
  ) {}

  ngOnInit(): void {
    const transactionId = this.activatedRoute.snapshot.paramMap.get('transactionId');

    if (!transactionId) {
      this.receiptLoadError.set('Payment receipt not found.');
      return;
    }

    this.loadReceipt(transactionId);
  }

  loadReceipt(transactionId: string): void {
    this.receiptLoading.set(true);
    this.receiptLoadError.set('');

    this.paymentService.getPaymentReceipt(transactionId).subscribe({
      next: (receipt) => {
        this.receipt.set(receipt);
        this.receiptLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.receiptLoading.set(false);
        this.receiptLoadError.set(
          resolveApiErrorMessage(error, 'Unable to load payment receipt.'),
        );
      },
    });
  }
}

import { HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_PATHS } from '../config/api-paths';
import { ApiService } from './api.service';
import {
  PaymentHistoryItem,
  PaymentReceipt,
  SendPaymentRequest,
} from '../../shared/models';

@Injectable({
  providedIn: 'root',
})
export class PaymentService {
  constructor(private apiService: ApiService) {}

  sendPayment(request: SendPaymentRequest, idempotencyKey: string): Observable<PaymentReceipt> {
    const headers = new HttpHeaders({
      'Idempotency-Key': idempotencyKey,
    });

    return this.apiService.post<PaymentReceipt>(API_PATHS.payments.send, request, headers);
  }

  getSentPayments(): Observable<PaymentHistoryItem[]> {
    return this.apiService.get<PaymentHistoryItem[]>(API_PATHS.payments.sent);
  }

  getReceivedPayments(): Observable<PaymentHistoryItem[]> {
    return this.apiService.get<PaymentHistoryItem[]>(API_PATHS.payments.received);
  }

  getPaymentHistory(): Observable<PaymentHistoryItem[]> {
    return this.apiService.get<PaymentHistoryItem[]>(API_PATHS.payments.history);
  }

  getPaymentReceipt(transactionId: string): Observable<PaymentHistoryItem> {
    return this.apiService.get<PaymentHistoryItem>(API_PATHS.payments.byId(transactionId));
  }
}

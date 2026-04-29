import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreatePaymentRequest,
  PaymentResponse,
  VerifyPaymentResponse,
} from '../models/payment.model';

@Injectable({
  providedIn: 'root',
})
export class PaymentService {
  private apiUrl = `${environment.apiUrl}/payments`;

  constructor(private http: HttpClient) {}

  createPayment(orderId: number, paymentMethod: 'cash' | 'card' = 'card'): Observable<PaymentResponse> {
    const body: CreatePaymentRequest = { orderId, paymentMethod };
    return this.http.post<PaymentResponse>(this.apiUrl, body);
  }

  verifyPayment(orderId: number): Observable<VerifyPaymentResponse> {
    return this.http.get<VerifyPaymentResponse>(`${this.apiUrl}/verify/${orderId}`);
  }
}


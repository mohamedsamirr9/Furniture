import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SellerEarnings } from '../models/payment.model';
import { SellerPayout } from '../models/payment.model';

@Injectable({
  providedIn: 'root'
})
export class SellerEarningsService {
  private readonly apiUrl = `${environment.apiUrl}/sellers`;

  constructor(private http: HttpClient) {}

  getEarnings(): Observable<SellerEarnings> {
    return this.http.get<SellerEarnings>(`${this.apiUrl}/earnings`);
  }

  getPayouts(): Observable<SellerPayout[]> {
    return this.http.get<SellerPayout[]>(`${this.apiUrl}/payouts`);
  }
}

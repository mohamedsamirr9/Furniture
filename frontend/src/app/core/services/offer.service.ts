import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class OfferService {
  private baseUrl = `${environment.apiUrl}/offers`;

  constructor(private http: HttpClient) {}

  createOffer(data: { customRequestId: number, price: number, deliveryDays: number }): Observable<any> {
    return this.http.post<any>(this.baseUrl, data);
  }

  getOffersByRequest(requestId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/request/${requestId}`);
  }

  getMyOffers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/my`);
  }

  getOfferById(offerId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${offerId}`);
  }

  acceptOffer(offerId: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/${offerId}/accept`, {});
  }
}

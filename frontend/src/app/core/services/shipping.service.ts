import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ShippingService {
  private baseUrl = 'http://localhost:5227/api/shipping';

  constructor(private http: HttpClient) {}

  estimateShipping(city: string, offerId?: number | null): Observable<{ shippingCost: number }> {
    let params = new HttpParams().set('city', city);
    if (offerId) {
      params = params.set('offerId', offerId.toString());
    }
    return this.http.get<{ shippingCost: number }>(`${this.baseUrl}/estimate`, { params });
  }
}

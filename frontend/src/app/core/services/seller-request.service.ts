import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SellerRequestDto } from '../models/seller-request.model';

@Injectable({ providedIn: 'root' })
export class SellerRequestService {
  private readonly sellerBase = `${environment.apiUrl}/seller`;
  private readonly adminBase = `${environment.apiUrl}/admin/seller-requests`;

  constructor(private http: HttpClient) {}

  getMyRequest(): Observable<SellerRequestDto | null> {
    return this.http.get<SellerRequestDto | null>(`${this.sellerBase}/my-request`);
  }

  getForAdmin(status: 'Pending' | 'Approved' | 'Rejected'): Observable<SellerRequestDto[]> {
    const params = new HttpParams().set('status', status);
    return this.http.get<SellerRequestDto[]>(this.adminBase, { params });
  }

  approve(id: number): Observable<void> {
    return this.http.post<void>(`${this.adminBase}/${id}/approve`, {});
  }

  reject(id: number, reason: string): Observable<void> {
    return this.http.post<void>(`${this.adminBase}/${id}/reject`, { reason });
  }
}

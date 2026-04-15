import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Complaint, CreateComplaint, UpdateComplaint } from '../models/complaint.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ComplaintService {
  private apiUrl = `${environment.apiUrl}/complaints`;

  constructor(private http: HttpClient) {}

  getComplaints(userId?: string): Observable<Complaint[]> {
    let params = new HttpParams();
    if (userId) {
      params = params.set('userId', userId);
    }
    return this.http.get<Complaint[]>(this.apiUrl, { params });
  }

  getAllComplaints(): Observable<Complaint[]> {
    return this.http.get<Complaint[]>(this.apiUrl);
  }

  getSellerComplaints(): Observable<Complaint[]> {
    return this.http.get<Complaint[]>(`${this.apiUrl}/seller`);
  }

  getComplaintById(id: number): Observable<Complaint> {
    return this.http.get<Complaint>(`${this.apiUrl}/${id}`);
  }
  getMyComplaints(): Observable<Complaint[]> {
    return this.http.get<Complaint[]>(`${this.apiUrl}/My`);
  }

  createComplaint(data: CreateComplaint): Observable<Complaint> {
    return this.http.post<Complaint>(this.apiUrl, data);
  }

  updateComplaint(id: number, data: UpdateComplaint): Observable<Complaint> {
    return this.http.put<Complaint>(`${this.apiUrl}/${id}`, data);
  }

  deleteComplaint(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}

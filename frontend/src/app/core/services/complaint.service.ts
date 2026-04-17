import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  Complaint,
  ComplaintDetail,
  ComplaintReply,
  CreateComplaint,
  ReplyComplaint,
  UpdateComplaint,
  UpdateComplaintStatus,
} from '../models/complaint.model';
import { environment } from '../../../environments/environment';

export interface ComplaintImageUploadResponse {
  secure_url: string;
  public_id: string;
}

@Injectable({
  providedIn: 'root',
})
export class ComplaintService {
  private apiUrl = `${environment.apiUrl}/complaints`;
  private readonly cloudinaryUploadUrl = 'https://api.cloudinary.com/v1_1/dcsd2lm6l/image/upload';
  private readonly complaintUploadPreset = 'complaint_images';

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

  getComplaintById(id: number): Observable<ComplaintDetail> {
    return this.http.get<ComplaintDetail>(`${this.apiUrl}/${id}`);
  }
  getMyComplaints(): Observable<Complaint[]> {
    return this.http.get<Complaint[]>(`${this.apiUrl}/My`);
  }

  createComplaint(data: CreateComplaint): Observable<Complaint> {
    return this.http.post<Complaint>(this.apiUrl, data);
  }

  uploadImage(file: File): Observable<ComplaintImageUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('upload_preset', this.complaintUploadPreset);
    return this.http.post<ComplaintImageUploadResponse>(this.cloudinaryUploadUrl, formData);
  }

  updateComplaint(id: number, data: UpdateComplaint): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, data);
  }

  updateComplaintStatus(id: number, data: UpdateComplaintStatus): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, data);
  }

  addReply(id: number, data: ReplyComplaint): Observable<ComplaintReply> {
    return this.http.post<ComplaintReply>(`${this.apiUrl}/${id}/replies`, data);
  }

  getReplies(id: number): Observable<ComplaintReply[]> {
    return this.http.get<ComplaintReply[]>(`${this.apiUrl}/${id}/replies`);
  }

  deleteComplaint(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}

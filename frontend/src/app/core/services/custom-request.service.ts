import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CustomRequestService {
  private baseUrl = 'http://localhost:5227/api/CustomRequest';

  constructor(private http: HttpClient) {}

  createCustomRequest(data: any): Observable<any> {
    return this.http.post<any>(this.baseUrl, data);
  }

  getMyRequests(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/my`);
  }

  uploadImage(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('upload_preset', 'product_images'); // Reusing existing preset

    return this.http.post<any>('https://api.cloudinary.com/v1_1/dcsd2lm6l/image/upload', formData);
  }
}

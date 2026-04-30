import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CustomRequestService {
  private baseUrl = `${environment.apiUrl}/CustomRequest`;

  constructor(private http: HttpClient) {}

  createCustomRequest(data: any): Observable<any> {
    return this.http.post<any>(this.baseUrl, data);
  }

  getAllRequests(): Observable<any> {
    return this.http.get<any>(this.baseUrl);
  }

  getMyRequests(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/my`);
  }

  uploadImage(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('upload_preset', 'product_images');  

    return this.http.post<any>('https://api.cloudinary.com/v1_1/dcsd2lm6l/image/upload', formData);
  }

  getRequestById(id: number): Observable<any> {
  return this.http.get<any>(`${this.baseUrl}/${id}`);
}

}

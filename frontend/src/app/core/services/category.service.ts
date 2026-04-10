import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Category } from '../models/category.model';

@Injectable({
  providedIn: 'root',
})
export class CategoryService {
  private apiUrl = 'http://localhost:5227/api/Categories';

  constructor(private http: HttpClient) {}

  getAllCategories(
    pageIndex: number = 1,
    pageSize: number = 10,
    search: string = '',
  ): Observable<any> {
    let params = new HttpParams()
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    if (search) {
      params = params.set('search', search);
    }

    return this.http.get<any>(this.apiUrl, { params });
  }

  getCategoryById(id: number): Observable<Category> {
    return this.http.get<Category>(`${this.apiUrl}/${id}`);
  }

  createCategory(data: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, data);
  }

  updateCategory(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, data);
  }

  deleteCategory(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }
  uploadImage(file: File) {
    const formData = new FormData();

    formData.append('file', file);
    formData.append('upload_preset', 'category_images');

    return this.http.post<any>('https://api.cloudinary.com/v1_1/dcsd2lm6l/image/upload', formData);
  }
}

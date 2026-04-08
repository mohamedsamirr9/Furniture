import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { ProductQueryParams } from '../models/product-query-params.model';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private baseUrl = 'http://localhost:5227/api';

  constructor(private http: HttpClient) {}

  buildHttpParams(filters: ProductQueryParams): HttpParams {
    let params = new HttpParams();
    Object.keys(filters).forEach((key) => {
      const value = (filters as any)[key];
      if (value !== null && value !== undefined && value !== '') {
        params = params.set(key, value);
      }
    });
    return params;
  }

  getProducts(filters: ProductQueryParams): Observable<any> {
    const params = this.buildHttpParams(filters);
    return this.http.get<any>(`${this.baseUrl}/Product`, { params });
  }

  getProductById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Product/${id}`);
  }

  getCategories(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/categories`);
  }

  getProductsByCategory(categoryId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/categories/${categoryId}`);
  }

  searchProducts(query: string, offset: number = 0, limit: number = 10): Observable<any> {
    return this.http.get<any>(
      `${this.baseUrl}/products/?title=${query}&offset=${offset}&limit=${limit}`,
    );
  }

  createProduct(data: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/Product`, data);
  }

  updateProduct(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/Product/${id}`, data);
  }

  deleteProduct(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/Product/${id}`);
  }
  uploadImage(file: File) {
    const formData = new FormData();

    formData.append('file', file);
    formData.append('upload_preset', 'product_images');

    return this.http.post<any>('https://api.cloudinary.com/v1_1/dcsd2lm6l/image/upload', formData);
  }
}

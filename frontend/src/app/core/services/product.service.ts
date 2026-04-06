import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private baseUrl = 'http://localhost:5227/api';

  constructor(private http: HttpClient) {}

  getProducts(params: any): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Product`, { params });
  }

  getProductById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Product/${id}`);
  }

  getCategories(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/categories`);
  }

  getProductsByCategory(
    categoryId: number,
  ): Observable<any> {
    return this.http.get<any>(
      `${this.baseUrl}/categories/${categoryId}`,
    );
  }

  searchProducts(query: string, offset: number = 0, limit: number = 10): Observable<any> {
    return this.http.get<any>(
      `${this.baseUrl}/products/?title=${query}&offset=${offset}&limit=${limit}`,
    );
  }
}

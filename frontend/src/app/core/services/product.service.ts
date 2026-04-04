import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private baseUrl = 'https://api.escuelajs.co/api/v1';

  constructor(private http: HttpClient) {}

  getProducts(params: any): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/products`, { params });
  }

  getProductById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/products/${id}`);
  }

  getCategories(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/categories`);
  }

  getProductsByCategory(
    categoryId: number,
    offset: number = 0,
    limit: number = 10,
  ): Observable<any> {
    return this.http.get<any>(
      `${this.baseUrl}/categories/${categoryId}/products?offset=${offset}&limit=${limit}`,
    );
  }

  searchProducts(query: string, offset: number = 0, limit: number = 10): Observable<any> {
    return this.http.get<any>(
      `${this.baseUrl}/products/?title=${query}&offset=${offset}&limit=${limit}`,
    );
  }
}

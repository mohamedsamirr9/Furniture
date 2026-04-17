import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { ProductQueryParams } from '../models/product-query-params.model';
import { Product } from '../models/product.model';

export interface ImageSearchResult {
  productId: number;
  name: string;
  price: number;
  similarity: number;
  imageUrl: string;
  description?: string;
}

export interface ImageSearchResponse {
  success: boolean;
  message: string;
  data: ImageSearchResult[];
}

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

  getProductById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/Product/${id}`);
  }

  getSellerProducts(filters: ProductQueryParams): Observable<any> {
    const params = this.buildHttpParams(filters);
    return this.http.get<any>(`${this.baseUrl}/seller/products`, { params });
  }

  getCategories(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/categories`);
  }

  getProductsByCategory(categoryId: number): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}/categories/${categoryId}`);
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

 
  searchByImage(file: File, topK: number = 10): Observable<any> {
    const formData = new FormData();
    formData.append('image', file);

    return this.http.post<any>(`${this.baseUrl}/search?topK=${topK}`, formData);
  }

 
  validateImageFile(file: File): { valid: boolean; error?: string } {
    const maxSize = 10 * 1024 * 1024; // 10MB
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp', 'image/jpg'];
    const allowedExtensions = ['.jpg', '.jpeg', '.png', '.webp'];

    if (!file || file.size === 0) {
      return { valid: false, error: 'Please select an image file' };
    }

    if (file.size > maxSize) {
      return { valid: false, error: 'File size must be less than 10MB' };
    }

    if (!allowedTypes.includes(file.type.toLowerCase())) {
      return { valid: false, error: 'Only JPEG, PNG, and WebP images are allowed' };
    }

    const extension = '.' + (file.name.split('.').pop()?.toLowerCase() || '');
    if (!allowedExtensions.includes(extension)) {
      return { valid: false, error: 'Invalid file extension' };
    }

    return { valid: true };
  }

  /**
   * Convert file to base64 for preview
   */
  fileToBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = (error) => reject(error);
    });
  }
}

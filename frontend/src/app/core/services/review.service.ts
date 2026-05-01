import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ReviewCreateDto {
  rating: number;
  message?: string;
  productId: number;
}

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private apiUrl = `${environment.apiUrl}/reviews`;

  constructor(private http: HttpClient) {}

  createReview(dto: ReviewCreateDto): Observable<any> {
    return this.http.post<any>(this.apiUrl, dto);
  }

  getMyReviewedProductIds(): Observable<number[]> {
    return this.http.get<number[]>(`${this.apiUrl}/my/products`);
  }

  getProductReviews(productId: number): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/products/${productId}/reviews`);
  }
}

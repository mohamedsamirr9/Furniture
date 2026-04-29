import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface ReviewCreateDto {
  rating: number;
  message?: string;
  productId: number;
  userId: string;
}

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private apiUrl = 'http://localhost:5227/api/reviews';

  constructor(private http: HttpClient) {}

  createReview(dto: Omit<ReviewCreateDto, 'userId'>): Observable<any> {
    const payload: ReviewCreateDto = {
      ...dto,
      userId: 'seller-1' // Mocking user for current dev environment
    };
    return this.http.post<any>(this.apiUrl, payload);
  }

  getMyReviewedProductIds(): Observable<number[]> {
    return this.http.get<number[]>(`${this.apiUrl}/my/products`);
  }

  getProductReviews(productId: number): Observable<any[]> {
    return this.http.get<any[]>(`http://localhost:5227/api/products/${productId}/reviews`);
  }
}

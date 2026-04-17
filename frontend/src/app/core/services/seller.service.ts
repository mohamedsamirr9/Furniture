import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SellerPortfolioItemDto {
  id: number;
  category: string;
  title: string;
  description: string;
  imageUrl: string;
}

export interface SellerProfileDto {
  id: string;
  name: string;
  location: string;
  joinDate: string;
  rating: number;
  reviewsCount: number;
  completedOrders: number;
  bio: string;
  avatarUrl: string;
  specialties: string[];
  portfolio: SellerPortfolioItemDto[];
}

@Injectable({
  providedIn: 'root',
})
export class SellerService {
  private readonly baseUrl = `${environment.apiUrl}/sellers`;

  constructor(private http: HttpClient) {}

  getSellerById(id: string): Observable<SellerProfileDto> {
    return this.http.get<SellerProfileDto>(`${this.baseUrl}/${id}`);
  }
}

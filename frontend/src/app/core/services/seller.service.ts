import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  mapSellerProfileApiToView,
  SellerProfileApiDto,
  SellerProfileViewModel,
  UpdateSellerProfilePayload,
} from '../models/seller-profile.model';

export type { SellerPortfolioItem, SellerProfileViewModel, UpdateSellerProfilePayload } from '../models/seller-profile.model';

/** @deprecated Use SellerProfileViewModel / SellerProfileApiDto from core/models/seller-profile.model */
export type SellerProfileDto = SellerProfileApiDto;

@Injectable({
  providedIn: 'root',
})
export class SellerService {
  private readonly baseUrl = `${environment.apiUrl}/sellers`;
  private readonly cloudinaryUploadUrl = 'https://api.cloudinary.com/v1_1/dcsd2lm6l/image/upload';
  private readonly profileImageUploadPreset = 'product_images';

  constructor(private http: HttpClient) {}

  getSellerById(id: string): Observable<SellerProfileViewModel> {
    return this.http
      .get<SellerProfileApiDto>(`${this.baseUrl}/${id}`)
      .pipe(map(mapSellerProfileApiToView));
  }

  getMySellerProfile(): Observable<SellerProfileViewModel> {
    return this.http
      .get<SellerProfileApiDto>(`${this.baseUrl}/me`)
      .pipe(map(mapSellerProfileApiToView));
  }

  updateMyProfile(payload: UpdateSellerProfilePayload): Observable<SellerProfileViewModel> {
    return this.http
      .put<SellerProfileApiDto>(`${this.baseUrl}/me`, payload)
      .pipe(map(mapSellerProfileApiToView));
  }

  uploadProfileImage(file: File): Observable<{ secure_url: string }> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('upload_preset', this.profileImageUploadPreset);
    return this.http.post<{ secure_url: string }>(this.cloudinaryUploadUrl, formData);
  }
}

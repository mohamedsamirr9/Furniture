import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { finalize } from 'rxjs';
import { SellerProfileDto, SellerService } from '../../../../../core/services/seller.service';

interface PortfolioItem {
  id: number;
  category: string;
  title: string;
  description: string;
  imageUrl: string;
}

interface Seller {
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
  portfolio: PortfolioItem[];
}

@Component({
  selector: 'app-seller-profile',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './seller-profile.html',
  styleUrl: './seller-profile.css'
})
export class SellerProfileComponent implements OnInit {
  seller: Seller | null = null;
  isLoading = false;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private sellerService: SellerService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const sellerId = params.get('id');
      if (!sellerId) {
        this.seller = null;
        this.errorMessage = 'Invalid seller id.';
        return;
      }

      this.loadSeller(sellerId);
    });
  }

  private loadSeller(sellerId: string): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.sellerService
      .getSellerById(sellerId)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (sellerDto) => {
          this.seller = this.mapSeller(sellerDto);
        },
        error: () => {
          this.seller = null;
          this.errorMessage = 'Failed to load seller profile.';
        },
      });
  }

  private mapSeller(dto: SellerProfileDto): Seller {
    return {
      id: dto.id,
      name: dto.name,
      location: dto.location,
      joinDate: dto.joinDate,
      rating: dto.rating,
      reviewsCount: dto.reviewsCount,
      completedOrders: dto.completedOrders,
      bio: dto.bio,
      avatarUrl: dto.avatarUrl,
      specialties: dto.specialties ?? [],
      portfolio: (dto.portfolio ?? []).map((item) => ({
        id: item.id,
        category: item.category,
        title: item.title,
        description: item.description,
        imageUrl: item.imageUrl,
      })),
    };
  }
}
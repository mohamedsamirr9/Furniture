import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { RecommendationService } from '../../../../../core/services/recommendation.service';
import { ProductRecommendationDto } from '../../../../../core/models/recommendation.model';

@Component({
  selector: 'app-home-recommendations',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home-recommendations.html',
  styleUrls: ['./home-recommendations.css']
})
export class HomeRecommendationsComponent implements OnInit {
  products: ProductRecommendationDto[] = [];
  loading = true;
  hasData = false;
  skeletons = Array(4).fill(0);

  constructor(private recService: RecommendationService) {}

  ngOnInit(): void {
    const token = localStorage.getItem('token');
    if (!token) {
      this.loading = false;
      return;
    }

    this.recService.getRecommendations(8).subscribe({
      next: (data) => {
        this.products = data;
        this.hasData = data.length > 0;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  onProductClick(product: ProductRecommendationDto): void {
    this.recService.trackClick(product.id).subscribe();
  }
}
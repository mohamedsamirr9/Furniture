import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RecommendationService } from '../../../../../core/services/recommendation.service';
import { ProductRecommendationDto } from '../../../../../core/models/recommendation.model';

@Component({
  selector: 'app-recommendations-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './recommendations-list.html',
  styleUrls: ['./recommendations-list.css']
})
export class RecommendationsListComponent implements OnInit {
  products: ProductRecommendationDto[] = [];
  loading = true;
  error = '';
  skeletons = Array(6).fill(0);

  constructor(
    public router: Router,
    private recService: RecommendationService
  ) {}

  ngOnInit(): void {
    this.recService.getRecommendations(10).subscribe({
      next: (data) => {
        this.products = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load recommendations.';
        this.loading = false;
      }
    });
  }

  onProductClick(product: ProductRecommendationDto): void {
    this.recService.trackClick(product.id).subscribe({
      error: (err) => console.error('Failed to track click', err)
    });
  }

  retakeQuiz(): void {
    this.router.navigate(['/quiz']);
  }
}
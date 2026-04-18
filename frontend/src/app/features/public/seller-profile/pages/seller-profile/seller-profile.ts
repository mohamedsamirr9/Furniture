import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { finalize } from 'rxjs';
import { SellerProfileViewModel } from '../../../../../core/models/seller-profile.model';
import { SellerService } from '../../../../../core/services/seller.service';
import { SellerProfileDisplayComponent } from '../../../../../shared/components/seller-profile-display/seller-profile-display';

@Component({
  selector: 'app-seller-profile',
  standalone: true,
  imports: [CommonModule, RouterModule, SellerProfileDisplayComponent],
  templateUrl: './seller-profile.html',
  styleUrl: './seller-profile.css',
})
export class SellerProfileComponent implements OnInit {
  seller: SellerProfileViewModel | null = null;
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
        next: (s) => {
          this.seller = s;
        },
        error: () => {
          this.seller = null;
          this.errorMessage = 'Failed to load seller profile.';
        },
      });
  }
}

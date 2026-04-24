import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs';
import { SellerProfileViewModel } from '../../../../../core/models/seller-profile.model';
import { SellerService } from '../../../../../core/services/seller.service';
import { SellerProfileDisplayComponent } from '../../../../../shared/components/seller-profile-display/seller-profile-display';

@Component({
  selector: 'app-edit-profile',
  standalone: true,
  imports: [CommonModule, SellerProfileDisplayComponent],
  templateUrl: './edit-profile.html',
  styleUrl: './edit-profile.css',
})
export class EditProfile implements OnInit {
  seller: SellerProfileViewModel | null = null;
  isLoading = true;
  loadError = '';
  successFlash = false;

  constructor(private sellerService: SellerService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.loadError = '';
    this.sellerService
      .getMySellerProfile()
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (s) => {
          this.seller = s;
        },
        error: () => {
          this.seller = null;
          this.loadError = 'Unable to load your profile.';
        },
      });
  }

  onProfileUpdated(vm: SellerProfileViewModel): void {
    this.seller = vm;
    this.successFlash = true;
    setTimeout(() => (this.successFlash = false), 3000);
  }
}

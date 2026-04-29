import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs';
import { SellerProfileViewModel } from '../../../../../core/models/seller-profile.model';
import { SellerService } from '../../../../../core/services/seller.service';
import { SellerProfileDisplayComponent } from '../../../../../shared/components/seller-profile-display/seller-profile-display';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-edit-profile',
  standalone: true,
  imports: [CommonModule, SellerProfileDisplayComponent, TranslateModule],
  templateUrl: './edit-profile.html',
  styleUrl: './edit-profile.css',
})
export class EditProfile implements OnInit {
  seller: SellerProfileViewModel | null = null;
  isLoading = true;
  loadError = '';
  successFlash = false;

  constructor(
    private sellerService: SellerService,
    private translate: TranslateService
  ) {}

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
          this.loadError = this.translate.instant('ALERTS.LOAD_ERROR');
        },
      });
  }

  onProfileUpdated(vm: SellerProfileViewModel): void {
    this.seller = vm;
    this.successFlash = true;
    setTimeout(() => (this.successFlash = false), 3000);
  }
}

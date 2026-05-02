import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../../../../core/services/auth.service';
import { SellerRequestService } from '../../../../../core/services/seller-request.service';
import { SellerRequestDto } from '../../../../../core/models/seller-request.model';
import { resolvePublicAssetUrl } from '../../../../../core/utils/public-url.util';

@Component({
  selector: 'app-my-seller-request',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule],
  templateUrl: './my-seller-request.html',
  styleUrl: './my-seller-request.css',
})
export class MySellerRequestComponent implements OnInit {
  loading = true;
  error = '';
  request: SellerRequestDto | null = null;

  constructor(
    private sellerRequestService: SellerRequestService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const role = this.authService.getUserRole();
    if (role === 'seller') {
      this.router.navigate(['/seller/dashboard']);
      return;
    }
    if (role === 'admin') {
      this.router.navigate(['/admin/dashboard']);
      return;
    }

    this.sellerRequestService.getMyRequest().subscribe({
      next: (data) => {
        this.request = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'SELLER_REQUEST.LOAD_ERROR';
        this.loading = false;
      },
    });
  }

  imageUrl(path: string | null | undefined): string {
    return resolvePublicAssetUrl(path);
  }

  statusLabel(status: string): string {
    const s = (status || '').toLowerCase();
    if (s === 'pending') return 'SELLER_REQUEST.STATUS_PENDING';
    if (s === 'approved') return 'SELLER_REQUEST.STATUS_APPROVED';
    if (s === 'rejected') return 'SELLER_REQUEST.STATUS_REJECTED';
    return status;
  }
}

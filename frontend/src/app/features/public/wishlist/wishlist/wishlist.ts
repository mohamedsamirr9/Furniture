import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { LocalizedPricePipe } from '../../../../core/pipes/localized-price.pipe';
import { WishlistService } from '../../../../core/services/wishlist.service';
import { CartService } from '../../../../core/services/cart.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, LocalizedPricePipe],
  templateUrl: './wishlist.html',
  styleUrl: './wishlist.css',
  host: {
  'style': 'display: block; background-color: #f9f4ef; min-height: 100vh;'
}

})
export class WishlistComponent implements OnInit, OnDestroy {
  loading = true;
  wishlistItems: any[] = [];
  private subscriptions: Subscription[] = [];

  constructor(
    private wishlistService: WishlistService,
    private cartService: CartService
  ) {}

  ngOnInit() {
    this.subscriptions.push(
      this.wishlistService.wishlist$.subscribe((items: any[]) => {
        this.wishlistItems = items;
      })
    );

    this.wishlistService.getWishlist().subscribe({
      next: () => {
        this.loading = false;
      },
      error: (err: any) => {
        console.error('Error loading wishlist', err);
        this.loading = false;
      }
    });
  }

  ngOnDestroy() {
    this.subscriptions.forEach(s => s.unsubscribe());
  }

  viewProduct(id: number) {
    // Actually router link should be used in HTML, but keeping this for now
    alert('Go to product details for ID: ' + id);
  }

  removeItem(productId: number) {
    this.wishlistService.removeFromWishlist(productId).subscribe({
      next: () => {
      },
      error: (err: any) => {
        console.error('Error removing from wishlist', err);
      }
    });
  }



  getStars(): number[] {
    return [1, 2, 3, 4, 5];
  }
}
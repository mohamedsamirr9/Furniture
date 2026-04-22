import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { WishlistService } from '../../../../core/services/wishlist.service';
import { CartService } from '../../../../core/services/cart.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
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
        console.log(`Product ${productId} removed from wishlist`);
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
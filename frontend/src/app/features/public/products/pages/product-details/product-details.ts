import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CartService } from '../../../../../core/services/cart.service';
import { ProductService } from '../../../../../core/services/product.service';
import { WishlistService } from '../../../../../core/services/wishlist.service';   
import { ReviewService } from '../../../../../core/services/review.service';

import { Subscription } from 'rxjs';

import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-product-details',
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './product-details.html',
  styleUrl: './product-details.css',
})
export class ProductDetails implements OnInit {
  product: any = null;
  isLoading: boolean = false;
  notFoundMessage: string = '';

  isAdding: boolean = false;
  addedSuccess: boolean = false;
  errorMessage: string = '';

  isInWishlist: boolean = false;
  wishlistMessage: string = '';
  private wishlistSub: Subscription | null = null;

  productReviews: any[] = [];
  isLoadingReviews: boolean = false;
  averageRating: number = 0;

  constructor(
    private cartService: CartService,
    private productService: ProductService,
    private wishlistService: WishlistService,
    private reviewService: ReviewService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.loadProduct(+id);
        this.loadReviews(+id);
      } else {
        this.notFoundMessage = 'Invalid product ID.';
      }
    });

    this.wishlistSub = this.wishlistService.wishlist$.subscribe((items: any[]) => {
      if (this.product) {
        this.checkWishlistStatus(items);
      }
    });
  }

  ngOnDestroy(): void {
    if (this.wishlistSub) {
      this.wishlistSub.unsubscribe();
    }
  }

  checkWishlistStatus(items?: any[]): void {
    if (!this.product) return;
    
    // If items aren't provided, get current value
    if (!items) {
      this.wishlistService.wishlist$.subscribe((curr: any[]) => items = curr).unsubscribe();
    }
    
    this.isInWishlist = items?.some(item => item.productId === this.product.id) || false;
  }

  loadProduct(id: number) {
    this.isLoading = true;
    this.notFoundMessage = '';
    this.productService.getProductById(id).subscribe({
      next: (res: any) => {
        this.product = res;
        this.checkWishlistStatus();
        this.isLoading = false;
      },
      error: (err: any) => {
        this.isLoading = false;
        this.notFoundMessage = 'Product not found.';
        console.error(err);
      }
    });
  }

  loadReviews(productId: number): void {
    this.isLoadingReviews = true;
    this.reviewService.getProductReviews(productId).subscribe({
      next: (data) => {
        this.productReviews = data;
        this.calculateAverageRating();
        this.isLoadingReviews = false;
      },
      error: (err) => {
        console.error('Error loading reviews', err);
        this.isLoadingReviews = false;
      }
    });
  }

  calculateAverageRating(): void {
    if (this.productReviews.length === 0) {
      this.averageRating = 0;
      return;
    }
    const sum = this.productReviews.reduce((acc, curr) => acc + curr.rating, 0);
    this.averageRating = sum / this.productReviews.length;
  }

  addToCart() {
    if (!this.product) return;
    this.isAdding = true;
    this.addedSuccess = false;
    this.errorMessage = '';
    
    this.cartService.addToCart(this.product.id, 1).subscribe({
      next: () => {
        this.isAdding = false;
        this.addedSuccess = true;
        setTimeout(() => this.addedSuccess = false, 3000);
      },
      error: (err: any) => {
        this.isAdding = false;
        this.errorMessage = err.error?.error || 'Failed to add item to cart.';
        console.error(err);
      }
    });
  }

  toggleWishlist() {
  if (!this.product) return;

  if (this.isInWishlist) {
    this.wishlistService.removeFromWishlist(this.product.id).subscribe({
      next: () => {
        this.isInWishlist = false;
        this.showWishlistMessage('WISHLIST.REMOVED');
      },
      error: (err: any) => console.error('Failed to remove from wishlist', err)
    });
  } else {
    this.wishlistService.addToWishlist(this.product.id).subscribe({
      next: () => {
        this.isInWishlist = true;
        this.showWishlistMessage('WISHLIST.ADDED');
      },
      error: (err: any) => console.error('Failed to add to wishlist', err)
    });
  }
}
showWishlistMessage(message: string) {
  this.wishlistMessage = message;
  setTimeout(() => this.wishlistMessage = '', 3000);
}
}

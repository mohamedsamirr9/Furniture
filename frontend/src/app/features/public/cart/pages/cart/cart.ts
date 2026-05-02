import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { LocalizedPricePipe } from '../../../../../core/pipes/localized-price.pipe';
import { CartService } from '../../../../../core/services/cart.service';
import { AuthService } from '../../../../../core/services/auth.service';
import { Observable } from 'rxjs';
import { Cart } from '../../../../../core/models/cart.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, LocalizedPricePipe],
  templateUrl: './cart.html',
  styleUrl: './cart.css',
  host: {
  'style': 'display: block; background-color: #f9f4ef; min-height: 100vh;'
}
})
export class CartComponent implements OnInit {
  cart$: Observable<Cart | null>;
  isLoading: boolean = false;
  errorMsg: string = '';

  constructor(
    private cartService: CartService,
    private authService: AuthService,
    private router: Router
  ) {
    this.cart$ = this.cartService.cart$;
  }

  ngOnInit(): void {
    if (this.authService.isLoggedIn()) {
      this.loadCart();
    }
  }

  loadCart(): void {
    this.isLoading = true;
    this.errorMsg = '';
    this.cartService.loadCart().subscribe({
      next: () => {
        this.isLoading = false;
      },
      error: (err: any) => {
        this.isLoading = false;
        this.errorMsg = 'Failed to load cart items. Please try again later.';
        console.error(err);
      }
    });
  }

  updateQuantity(productId: number, currentQuantity: number, change: number): void {
    const newQuantity = currentQuantity + change;
    if (newQuantity < 1) return;

    this.isLoading = true;
    this.cartService.updateQuantity(productId, newQuantity).subscribe({
      next: () => this.isLoading = false,
      error: (err: any) => {
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  removeItem(productId: number): void {
    this.isLoading = true;
    this.cartService.removeCartItem(productId).subscribe({
      next: () => this.isLoading = false,
      error: (err: any) => {
        this.isLoading = false;
        this.errorMsg = 'Failed to remove item.';
        console.error(err);
      }
    });
  }

  clearCart(): void {
    if (confirm('Are you sure you want to clear your cart?')) {
      this.isLoading = true;
      this.cartService.clearCart().subscribe({
        next: () => this.isLoading = false,
        error: (err: any) => {
          this.isLoading = false;
          this.errorMsg = 'Failed to clear cart.';
          console.error(err);
        }
      });
    }
  }

  hasBlockedSeller(cart: Cart | null): boolean {
    return !!cart?.items?.some(item => this.isItemBlocked(item));
  }

  isItemBlocked(item: any): boolean {
    return !!(item?.sellerIsBlocked || item?.isBlocked);
  }

  proceedToCheckout(cart: Cart | null): void {
    if (this.hasBlockedSeller(cart)) {
      this.errorMsg = 'Checkout is unavailable because one or more sellers are blocked.';
      return;
    }
    this.router.navigate(['/checkout']);
  }
}

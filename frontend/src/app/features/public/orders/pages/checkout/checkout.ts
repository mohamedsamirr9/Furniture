import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CartService } from '../../../../../core/services/cart.service';
import { OrderService } from '../../../../../core/services/order.service';
import { OfferService } from '../../../../../core/services/offer.service';
import { of } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, TranslateModule],
  templateUrl: './checkout.html',
  styleUrls: ['./checkout.css']
})
export class CheckoutComponent implements OnInit {
  checkoutForm: FormGroup;
  isLoading = false;
  offerId: number | null = null;
  displayData$: Observable<any>;

  constructor(
    private fb: FormBuilder,
    private cartService: CartService,
    private orderService: OrderService,
    private offerService: OfferService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.displayData$ = this.route.queryParams.pipe(
      switchMap(params => {
        this.offerId = params['offerId'] ? +params['offerId'] : null;
        if (this.offerId) {
          return this.offerService.getOfferById(this.offerId).pipe(
            switchMap(offer => {
              // Transform offer into a cart-like structure for the UI
              return of({
                totalPrice: offer.price,
                items: [{
                  productName: offer.customRequest?.description || 'Custom Furniture Product',
                  productImage: offer.customRequest?.imageUrl,
                  unitPrice: offer.price,
                  quantity: 1
                }]
              });
            })
          );
        } else {
          return this.cartService.cart$;
        }
      })
    );
    this.checkoutForm = this.fb.group({
      shippingAddress: ['', [Validators.required, Validators.minLength(10)]],
      notes: ['']
    });
  }

  ngOnInit(): void {
    if (!this.offerId) {
      this.cartService.loadCart().subscribe();
    }
  }

  onSubmit(): void {
    if (this.checkoutForm.invalid) {
      this.checkoutForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const formValue = this.checkoutForm.value;

    if (this.offerId) {
      this.orderService.createOrderFromOffer({
        offerId: this.offerId,
        shippingAddress: formValue.shippingAddress,
        notes: formValue.notes
      }).subscribe({
        next: (response: any) => {
          this.isLoading = false;
          this.router.navigate(['/orders/confirmed'], { state: { orderResponse: response } });
        },
        error: (err) => {
          this.isLoading = false;
          console.error('Error creating order from offer', err);
          alert('Failed to place order. Please try again.');
        }
      });
    } else {
      this.orderService.createOrder({
        shippingAddress: formValue.shippingAddress,
        notes: formValue.notes
      }).subscribe({
        next: (response: any) => {
          this.isLoading = false;
          this.cartService.clearCart().subscribe(() => {
            this.router.navigate(['/orders/confirmed'], { state: { orderResponse: response } });
          });
        },
        error: (err) => {
          this.isLoading = false;
          console.error('Error creating order', err);
          alert('Failed to place order. Please try again.');
        }
      });
    }
  }
}

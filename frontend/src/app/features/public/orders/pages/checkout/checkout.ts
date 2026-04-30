import { Component, OnInit, OnDestroy, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CartService } from '../../../../../core/services/cart.service';
import { OrderService } from '../../../../../core/services/order.service';
import { PaymentService } from '../../../../../core/services/payment.service';
import { OfferService } from '../../../../../core/services/offer.service';
import { ShippingService } from '../../../../../core/services/shipping.service';
import { of, BehaviorSubject, Subject } from 'rxjs';
import { switchMap, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs/operators';

import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule, TranslateModule],
  templateUrl: './checkout.html',
  styleUrls: ['./checkout.css'],
  encapsulation: ViewEncapsulation.None
})
export class CheckoutComponent implements OnInit, OnDestroy {
  checkoutForm: FormGroup;
  isLoading = false;
  offerId: number | null = null;
  displayData$: Observable<any>;
  shippingCost$ = new BehaviorSubject<number>(0);
  paymentMethod: 'Cash' | 'Card' = 'Cash';
  hasBlockedSellerInOrder = false;
  cities: string[] = [
    'Cairo', 'Giza', 'Alexandria', 'Aswan', 'Luxor', 
    'Port Said', 'Suez', 'Mansoura', 'Tanta', 'Ismailia', 'Assiut'
  ];
  private destroy$ = new Subject<void>();

  private extractOrderIds(response: any): number[] {
    // Split-orders response shape: { orders: [{ orderId: ... }, ...] }
    const orders = response?.orders ?? response?.Orders;
    if (Array.isArray(orders)) {
      return orders
        .map((o: any) => o?.orderId ?? o?.OrderId ?? o?.id)
        .filter((id: any) => typeof id === 'number' && id > 0);
    }

    // Legacy single-order response shape
    const single = response?.orderId ?? response?.OrderId ?? response?.id;
    return typeof single === 'number' && single > 0 ? [single] : [];
  }

  constructor(
    private fb: FormBuilder,
    private cartService: CartService,
    private orderService: OrderService,
    private paymentService: PaymentService,
    private offerService: OfferService,
    private shippingService: ShippingService,
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
      city: ['', Validators.required],
      shippingAddress: ['', [Validators.required, Validators.minLength(10)]],
      notes: ['']
    });

    this.checkoutForm.get('city')?.valueChanges.pipe(
      takeUntil(this.destroy$),
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe((city: string | null) => {
      if (city) {
        this.shippingService.estimateShipping(city, this.offerId).subscribe({
          next: (res: any) => this.shippingCost$.next(res.shippingCost),
          error: (err: any) => {
            console.error('Failed to estimate shipping', err);
            this.shippingCost$.next(0);
          }
        });
      } else {
         this.shippingCost$.next(0);
      }
    });
  }

  ngOnInit(): void {
    if (!this.offerId) {
      this.cartService.loadCart().subscribe();
    }

    this.displayData$.pipe(takeUntil(this.destroy$)).subscribe((data: any) => {
      this.hasBlockedSellerInOrder = !!data?.items?.some((item: any) => item?.sellerIsBlocked);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSubmit(): void {
    if (this.checkoutForm.invalid) {
      this.checkoutForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const formValue = this.checkoutForm.value;

    if (this.hasBlockedSellerInOrder) {
      this.isLoading = false;
      alert('Checkout is unavailable because one or more sellers are blocked.');
      return;
    }

    if (this.offerId) {
      this.orderService.createOrderFromOffer({
        offerId: this.offerId,
        city: formValue.city,
        shippingAddress: formValue.shippingAddress,
        notes: formValue.notes,
        paymentMethod: this.paymentMethod
      } as any).subscribe({
        next: (response: any) => {
          this.isLoading = false;
          const orderId = response?.orderId ?? response?.OrderId ?? response?.id;
          if (this.paymentMethod === 'Card') {
            this.router.navigate(['/orders/pay'], {
              state: {
                orderId,
                orderResponse: response,
                paymentMethod: 'card'
              }
            });
          } else {
            this.paymentService.createPayment(orderId, 'cash').subscribe({
              next: () => {
                this.router.navigate(['/orders/confirmed'], {
                  state: {
                    orderId,
                    orderResponse: response
                  }
                });
              },
              error: (err: any) => {
                console.error('Error recording cash payment', err);
                alert(err?.error?.message || 'Failed to finalize cash order. Please try again.');
              }
            });
          }
        },
        error: (err: any) => {
          this.isLoading = false;
          console.error('Error creating order from offer', err);
          alert('Failed to place order. Please try again.');
        }
      });
    } else {
      this.orderService.createOrder({
        city: formValue.city,
        shippingAddress: formValue.shippingAddress,
        notes: formValue.notes,
        paymentMethod: this.paymentMethod
      } as any).subscribe({
        next: (response: any) => {
          this.isLoading = false;
          const orderIds = this.extractOrderIds(response);
          const orderId = orderIds[0];
          if (!orderId) {
            alert('Failed to place order. Please try again.');
            return;
          }

          if (this.paymentMethod === 'Card') {
            this.cartService.clearCart().subscribe(() => {
              this.router.navigate(['/orders/pay'], {
                state: {
                  orderId,
                  orderIds,
                  orderResponse: response,
                  paymentMethod: 'card'
                }
              });
            });
          } else {
            // Record cash payment per created order
            let completed = 0;
            let failed = false;

            orderIds.forEach((id: number) => {
              this.paymentService.createPayment(id, 'cash').subscribe({
                next: () => {
                  completed += 1;
                  if (!failed && completed === orderIds.length) {
                    this.cartService.clearCart().subscribe(() => {
                      this.router.navigate(['/orders/confirmed'], {
                        state: {
                          orderId,
                          orderIds,
                          orderResponse: response
                        }
                      });
                    });
                  }
                },
                error: (err: any) => {
                  if (failed) return;
                  failed = true;
                  console.error('Error recording cash payment', err);
                  alert(err?.error?.message || 'Failed to finalize cash order. Please try again.');
                }
              });
            });
          }
        },
        error: (err: any) => {
          this.isLoading = false;
          console.error('Error creating order', err);
          alert('Failed to place order. Please try again.');
        }
      });
    }
  }
}

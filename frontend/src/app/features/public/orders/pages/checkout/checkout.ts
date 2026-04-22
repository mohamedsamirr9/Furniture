import { Component, OnInit, OnDestroy, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CartService } from '../../../../../core/services/cart.service';
import { OrderService } from '../../../../../core/services/order.service';
import { OfferService } from '../../../../../core/services/offer.service';
import { ShippingService } from '../../../../../core/services/shipping.service';
import { of, BehaviorSubject, Subject } from 'rxjs';
import { switchMap, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs/operators';

import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, TranslateModule],
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
  cities: string[] = [
    'Cairo', 'Giza', 'Alexandria', 'Aswan', 'Luxor', 
    'Port Said', 'Suez', 'Mansoura', 'Tanta', 'Ismailia', 'Assiut'
  ];
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private cartService: CartService,
    private orderService: OrderService,
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

    if (this.offerId) {
      this.orderService.createOrderFromOffer({
        offerId: this.offerId,
        city: formValue.city,
        shippingAddress: formValue.shippingAddress,
        notes: formValue.notes
      }).subscribe({
        next: (response: any) => {
          this.isLoading = false;
          this.router.navigate(['/orders/confirmed'], { state: { orderResponse: response } });
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
        notes: formValue.notes
      }).subscribe({
        next: (response: any) => {
          this.isLoading = false;
          this.cartService.clearCart().subscribe(() => {
            this.router.navigate(['/orders/confirmed'], { state: { orderResponse: response } });
          });
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

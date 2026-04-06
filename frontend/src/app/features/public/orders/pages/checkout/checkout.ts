import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CartService } from '../../../../../core/services/cart.service';
import { OrderService } from '../../../../../core/services/order.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './checkout.html',
  styleUrls: ['./checkout.css']
})
export class CheckoutComponent implements OnInit {
  checkoutForm: FormGroup;
  isLoading = false;
  cart$: Observable<any>;

  constructor(
    private fb: FormBuilder,
    private cartService: CartService,
    private orderService: OrderService,
    private router: Router
  ) {
    this.cart$ = this.cartService.cart$;
    this.checkoutForm = this.fb.group({
      shippingAddress: ['', [Validators.required, Validators.minLength(10)]],
      notes: ['']
    });
  }

  ngOnInit(): void {
    this.cartService.loadCart().subscribe();
  }

  onSubmit(): void {
    if (this.checkoutForm.invalid) {
      this.checkoutForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const formValue = this.checkoutForm.value;

    this.orderService.createOrder({
      shippingAddress: formValue.shippingAddress,
      notes: formValue.notes
    }).subscribe({
      next: (response: any) => {
        this.isLoading = false;
        // Optionally clear the cart since checkout was successful
        this.cartService.clearCart().subscribe(() => {
          this.router.navigate(['/orders/confirmed'], { state: { orderResponse: response } });
        });
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Error creating order', err);
        alert('Failed to close order. Please try again.');
      }
    });
  }
}

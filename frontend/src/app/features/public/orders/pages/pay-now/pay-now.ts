import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { Subject, timer } from 'rxjs';
import { switchMap, takeUntil, takeWhile } from 'rxjs/operators';
import { TranslateModule } from '@ngx-translate/core';
import { PaymentService } from '../../../../../core/services/payment.service';

type OrderResponseLike = {
  orderId?: number;
  OrderId?: number;
  id?: number;
  totalPrice?: number;
  TotalPrice?: number;
};

@Component({
  selector: 'app-pay-now',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './pay-now.html',
  styleUrls: ['./pay-now.css'],
})
export class PayNowComponent implements OnInit, OnDestroy {
  isLoading = true;
  isVerifying = false;
  errorMessage: string | null = null;

  orderId: number | null = null;
  orderResponse: OrderResponseLike | null = null;

  paymentUrl: string | null = null;
  safePaymentUrl: SafeResourceUrl | null = null;

  isPaid: boolean | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private sanitizer: DomSanitizer,
    private paymentService: PaymentService,
  ) {
    const nav = this.router.getCurrentNavigation();
    if (nav?.extras.state) {
      this.orderResponse = (nav.extras.state['orderResponse'] as OrderResponseLike) ?? null;
      const passedOrderId = nav.extras.state['orderId'] as number | undefined;
      if (typeof passedOrderId === 'number') this.orderId = passedOrderId;
    }
  }

  ngOnInit(): void {
    const status = this.route.snapshot.queryParamMap.get('status');

    this.extractOrderId();

    if (status === 'failed') {
      this.isLoading = false;
      this.errorMessage = 'Payment failed. Please try again.';
      return;
    }

    if (this.orderId) {
      this.startPaymentFlow();
    } else {
      this.isLoading = false;
      this.errorMessage = 'Missing order id.';
    }
  }
  startPaymentFlow(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.paymentService
      .createPayment(this.orderId!)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          const url = (res as any).paymentUrl ?? (res as any).PaymentUrl;
          if (url) {
            this.safePaymentUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
          } else {
            this.errorMessage = 'paymentUrl is missing in the response.';
          }
          this.isLoading = false;
        },
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = err?.error?.message || 'An error occurred while initiating payment.';
        },
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  goToOrders(): void {
    this.router.navigate(['/orders']);
  }
  extractOrderId(): void {
    const raw = this.route.snapshot.queryParamMap.get('orderId');
    if (!this.orderId) {
      this.orderId = raw ? Number(raw) : (this.orderResponse?.id ?? null);
    }
  }
}

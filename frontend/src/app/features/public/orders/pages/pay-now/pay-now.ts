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
    private paymentService: PaymentService
  ) {
    const nav = this.router.getCurrentNavigation();
    if (nav?.extras.state) {
      this.orderResponse = (nav.extras.state['orderResponse'] as OrderResponseLike) ?? null;
      const passedOrderId = nav.extras.state['orderId'] as number | undefined;
      if (typeof passedOrderId === 'number') this.orderId = passedOrderId;
    }
  }

  ngOnInit(): void {
    // Fallback to query param: /orders/pay?orderId=123
    if (!this.orderId) {
      const raw = this.route.snapshot.queryParamMap.get('orderId');
      const parsed = raw ? Number(raw) : NaN;
      if (!Number.isNaN(parsed) && parsed > 0) this.orderId = parsed;
    }

    // If we have orderResponse, derive orderId from it.
    if (!this.orderId && this.orderResponse) {
      this.orderId =
        this.orderResponse.orderId ??
        this.orderResponse.OrderId ??
        this.orderResponse.id ??
        null;
    }

    if (!this.orderId) {
      this.isLoading = false;
      this.errorMessage = 'Missing order id.';
      return;
    }

    this.paymentService.createPayment(this.orderId).pipe(takeUntil(this.destroy$)).subscribe({
      next: (res) => {
        // Backend is PascalCase; HttpClient keeps keys as-is.
        const url = (res as any).paymentUrl ?? (res as any).PaymentUrl;
        this.paymentUrl = typeof url === 'string' ? url : null;

        if (!this.paymentUrl) {
          this.isLoading = false;
          this.errorMessage = 'Payment URL is missing from server response.';
          return;
        }

        this.safePaymentUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.paymentUrl);
        this.isLoading = false;
        this.startVerifyPolling();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err?.error?.message || 'Failed to start payment.';
      },
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  startVerifyPolling(): void {
    if (!this.orderId) return;

    this.isVerifying = true;
    this.errorMessage = null;
    this.isPaid = null;

    const maxAttempts = 40; // ~2 minutes at 3s interval

    timer(0, 3000)
      .pipe(
        takeUntil(this.destroy$),
        takeWhile((attempt) => attempt < maxAttempts, true),
        switchMap(() => this.paymentService.verifyPayment(this.orderId!))
      )
      .subscribe({
        next: (res) => {
          const paid = (res as any).isPaid ?? (res as any).IsPaid;
          this.isPaid = !!paid;

          if (this.isPaid) {
            this.isVerifying = false;
            this.router.navigate(['/orders/confirmed'], {
              state: {
                orderResponse: this.orderResponse ?? { orderId: this.orderId },
              },
            });
          }
        },
        error: (err) => {
          this.isVerifying = false;
          this.errorMessage = err?.error?.message || 'Failed to verify payment status.';
        },
        complete: () => {
          if (!this.isPaid) {
            this.isVerifying = false;
          }
        },
      });
  }

  checkAgain(): void {
    this.startVerifyPolling();
  }

  goToOrders(): void {
    this.router.navigate(['/orders']);
  }
}


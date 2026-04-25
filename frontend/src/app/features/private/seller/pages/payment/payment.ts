import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { SellerEarningsService } from '../../../../../core/services/seller-earnings.service';
import { OrderService } from '../../../../../core/services/order.service';
import { SellerService } from '../../../../../core/services/seller.service';
import { SellerPayout, SellerEarnings } from '../../../../../core/models/payment.model';
import { Order } from '../../../../../core/models/order.model';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './payment.html',
  styleUrl: './payment.css',
})
export class Payment implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  earnings: SellerEarnings = {
    totalSales: 0,
    totalCommission: 0,
    netEarnings: 0,
    pendingAmount: 0,
    paidAmount: 0,
  };

  isLoading = true;
  errorMessage: string | null = null;
  saveSuccess = false;

  orders: Order[] = [];
  ordersLoading = false;

  recentPayouts: SellerPayout[] = [];
  payoutsLoading = false;

  bankForm = {
    bankName: '',
    bankAccountNumber: '',
    bankCode: '',
    nationalId: '',
    paymobMerchantId: '',
    verified: false,
  };

  constructor(
    private earningsService: SellerEarningsService,
    private orderService: OrderService,
    private sellerService: SellerService,
    private translate: TranslateService,
  ) {}

  ngOnInit(): void {
    this.loadEarnings();
    this.loadOrders();
    this.loadPayouts();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadEarnings(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.earningsService
      .getEarnings()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.earnings = data;
          this.isLoading = false;
        },
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = this.translate.instant('SELLER.PAYMENTS.ERRORS.LOAD_FAILED');
          console.error('Earnings load error:', err);
        },
      });
  }

  loadOrders(): void {
    this.ordersLoading = true;
    this.orderService
      .getSellerOrders()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.orders = data;
          this.ordersLoading = false;
        },
        error: (err) => {
          this.ordersLoading = false;
          console.error('Orders load error:', err);
        },
      });
  }

  loadPayouts(): void {
    this.payoutsLoading = true;
    this.earningsService
      .getPayouts()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.recentPayouts = data.slice(0, 5);
          this.payoutsLoading = false;
        },
        error: (err) => {
          this.payoutsLoading = false;
          console.error('Payouts load error:', err);
        },
      });
  }

  saveBankDetails(): void {
    this.saveSuccess = false;
    this.errorMessage = null;
    const payload = {
      bankName: this.bankForm.bankName ?? undefined,
      bankAccountNumber: this.bankForm.bankAccountNumber ?? undefined,
      bankCode: this.bankForm.bankCode ?? undefined,
      nationalId: this.bankForm.nationalId ?? undefined,
      paymobMerchantId: this.bankForm.paymobMerchantId ?? undefined,
    };
    this.sellerService
      .updateMyProfile(payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.saveSuccess = true;
          setTimeout(() => (this.saveSuccess = false), 3000);
        },
        error: (err) => {
          this.errorMessage = this.translate.instant('SELLER.BANK.ERRORS.SAVE_FAILED');
          console.error('Save bank details error:', err);
        },
      });
  }

  get netEarnings(): number {
    return this.earnings.netEarnings || 0;
  }

  get pendingAmount(): number {
    return this.earnings.pendingAmount || 0;
  }

  get lastPayoutAmount(): number {
    return this.recentPayouts.length > 0 ? this.recentPayouts[0].amount : 0;
  }
}

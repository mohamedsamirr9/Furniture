import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject, takeUntil } from 'rxjs';
import { SellerEarningsService } from '../../../../../core/services/seller-earnings.service';
import { SellerService } from '../../../../../core/services/seller.service';
import { RecentPayout, SellerPaymentDashboard } from '../../../../../core/models/payment.model';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './payment.html',
  styleUrl: './payment.css',
})
export class Payment implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  dashboard: SellerPaymentDashboard | null = null;

  isLoading = true;
  errorMessage: string | null = null;
  saveSuccess = false;
  recentPayouts: RecentPayout[] = [];

  bankForm = {
    bankName: '',
    bankAccountNumber: '',
    bankCode: '',
    nationalId: '',
    paymobMerchantId: '',
    verified: false,
  };

  hasExistingBankDetails = false;


  constructor(
    private earningsService: SellerEarningsService,
    private sellerService: SellerService,
    private translate: TranslateService,
  ) {}

  ngOnInit(): void {
    this.refreshPaymentData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private refreshPaymentData(): void {
    this.loadDashboard();
  }

  retryLoad(): void {
    this.refreshPaymentData();
  }

  loadDashboard(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.earningsService
      .getPaymentDashboard()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.dashboard = data;
          this.recentPayouts = data.recentPayouts || [];
          this.isLoading = false;
        },
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = this.translate.instant('SELLER.PAYMENTS.ERRORS.LOAD_FAILED');
          console.error('Payment dashboard load error:', err);
        }
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
          this.hasExistingBankDetails = true;
          setTimeout(() => (this.saveSuccess = false), 3000);
        },
        error: (err) => {
          this.errorMessage = this.translate.instant('SELLER.BANK.ERRORS.SAVE_FAILED');
          console.error('Save bank details error:', err);
        },
      });
  }

  get netEarnings(): number {
    return this.dashboard?.onlineEarnings.totalEarnings || 0;
  }

  get pendingAmount(): number {
    return this.dashboard?.onlineEarnings.pendingPayout || 0;
  }

  get paidAmount(): number {
    return this.dashboard?.onlineEarnings.totalPaid || 0;
  }

  get totalCashAmount(): number {
    return this.dashboard?.cashSummary.cashAmount || 0;
  }

  get totalCashOrders(): number {
    return this.dashboard?.cashSummary.totalCashOrders || 0;
  }

  get remainingLimit(): number {
    return this.dashboard?.cashSummary.remainingLimit || 0;
  }

  get pendingCommission(): number {
    return this.dashboard?.cashSummary.pendingCommission || 0;
  }

  get maxLimit(): number {
    return this.dashboard?.cashSummary.maxLimit || 0;
  }

  get isBlocked(): boolean {
    return this.remainingLimit <= 0;
  }
}

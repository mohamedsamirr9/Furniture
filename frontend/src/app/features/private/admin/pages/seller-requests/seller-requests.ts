import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { SellerRequestService } from '../../../../../core/services/seller-request.service';
import { SellerRequestDto } from '../../../../../core/models/seller-request.model';
import { getApiErrorMessage } from '../../../../../core/utils/api-error.util';
import { resolvePublicAssetUrl } from '../../../../../core/utils/public-url.util';

type SellerRequestAdminTab = 'Pending' | 'Approved' | 'Rejected';

@Component({
  selector: 'app-seller-requests',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './seller-requests.html',
  styleUrl: './seller-requests.css',
})
export class SellerRequests implements OnInit {
  readonly tabs: SellerRequestAdminTab[] = ['Pending', 'Approved', 'Rejected'];

  activeTab: SellerRequestAdminTab = 'Pending';
  requests: SellerRequestDto[] = [];
  loading = true;
  loadError = '';

  actionId: number | null = null;
  rejectModalOpen = false;
  rejectReason = '';
  rejectTargetId: number | null = null;
  rejectSubmitting = false;

  toast: { type: 'success' | 'error'; text: string } | null = null;

  constructor(
    private sellerRequestService: SellerRequestService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  selectTab(tab: SellerRequestAdminTab): void {
    if (this.activeTab === tab) return;
    this.activeTab = tab;
    this.load();
  }

  tabLabelKey(tab: SellerRequestAdminTab): string {
    switch (tab) {
      case 'Pending':
        return 'ADMIN.SELLER_TAB_PENDING';
      case 'Approved':
        return 'ADMIN.SELLER_TAB_APPROVED';
      case 'Rejected':
        return 'ADMIN.SELLER_TAB_REJECTED';
    }
  }

  load(): void {
    this.loading = true;
    this.loadError = '';
    this.sellerRequestService.getForAdmin(this.activeTab).subscribe({
      next: (data) => {
        this.requests = data ?? [];
        this.loading = false;
      },
      error: (err) => {
        this.loadError = getApiErrorMessage(err, 'Failed to load seller requests');
        this.loading = false;
      },
    });
  }

  imageUrl(path: string | null | undefined): string {
    return resolvePublicAssetUrl(path);
  }

  emptyMessageKey(): string {
    switch (this.activeTab) {
      case 'Pending':
        return 'ADMIN.SELLER_REQ_EMPTY_PENDING';
      case 'Approved':
        return 'ADMIN.SELLER_REQ_EMPTY_APPROVED';
      case 'Rejected':
        return 'ADMIN.SELLER_REQ_EMPTY_REJECTED';
    }
  }

  countSuffixKey(): string {
    switch (this.activeTab) {
      case 'Pending':
        return 'ADMIN.SELLER_REQ_COUNT_SUFFIX_PENDING';
      case 'Approved':
        return 'ADMIN.SELLER_REQ_COUNT_SUFFIX_APPROVED';
      case 'Rejected':
        return 'ADMIN.SELLER_REQ_COUNT_SUFFIX_REJECTED';
    }
  }

  statusBadgeClass(status: string | undefined): string {
    const s = (status ?? '').toLowerCase();
    if (s === 'pending') return 'badge text-bg-warning';
    if (s === 'approved') return 'badge text-bg-success';
    if (s === 'rejected') return 'badge text-bg-danger';
    return 'badge text-bg-secondary';
  }

  statusLabelKey(status: string | undefined): string {
    const s = (status ?? '').toLowerCase();
    if (s === 'pending') return 'SELLER_REQUEST.STATUS_PENDING';
    if (s === 'approved') return 'SELLER_REQUEST.STATUS_APPROVED';
    if (s === 'rejected') return 'SELLER_REQUEST.STATUS_REJECTED';
    return status ?? '';
  }

  approve(id: number): void {
    if (this.actionId !== null) return;
    this.actionId = id;
    this.sellerRequestService.approve(id).subscribe({
      next: () => {
        this.actionId = null;
        this.showToast('success', this.translate.instant('ADMIN.SELLER_REQ_APPROVED_OK'));
        this.load();
      },
      error: (err) => {
        this.actionId = null;
        this.showToast('error', getApiErrorMessage(err, 'Approve failed'));
      },
    });
  }

  openReject(id: number): void {
    this.rejectTargetId = id;
    this.rejectReason = '';
    this.rejectModalOpen = true;
  }

  closeRejectModal(): void {
    if (this.rejectSubmitting) return;
    this.rejectModalOpen = false;
    this.rejectTargetId = null;
  }

  submitReject(): void {
    const id = this.rejectTargetId;
    if (id === null || !this.rejectReason.trim()) return;
    this.rejectSubmitting = true;
    this.sellerRequestService.reject(id, this.rejectReason.trim()).subscribe({
      next: () => {
        this.rejectSubmitting = false;
        this.rejectModalOpen = false;
        this.rejectTargetId = null;
        this.showToast('success', this.translate.instant('ADMIN.SELLER_REQ_REJECTED_OK'));
        this.load();
      },
      error: (err) => {
        this.rejectSubmitting = false;
        this.showToast('error', getApiErrorMessage(err, 'Reject failed'));
      },
    });
  }

  private showToast(type: 'success' | 'error', text: string): void {
    this.toast = { type, text };
    setTimeout(() => (this.toast = null), 4000);
  }
}

import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { Order } from '../../../core/models/order.model';
import { LocalizedPricePipe } from '../../../core/pipes/localized-price.pipe';

@Component({
  selector: 'app-order-details-modal',
  standalone: true,
  imports: [CommonModule, TranslateModule, LocalizedPricePipe],
  templateUrl: './order-details-modal.html',
  styleUrl: './order-details-modal.css',
})
export class OrderDetailsModalComponent {
  @Input() isOpen = false;
  @Input() loading = false;
  @Input() error = '';
  @Input() order: Order | null = null;
  @Output() close = new EventEmitter<void>();

  onClose() {
    this.close.emit();
  }

  lineTotal(line: { unitPrice: number; quantity: number; total?: number }): number {
    if (line.total != null && !Number.isNaN(Number(line.total))) {
      return Number(line.total);
    }
    return line.unitPrice * line.quantity;
  }

  getDisplayStatus(order: Order): string {
    const paymentStatus = this.normalizePaymentStatus(order.paymentStatus);
    const paymentMethod = (order.paymentMethod || '').toLowerCase();
    if ((paymentStatus === 'unpaid' || paymentMethod === 'cash') && order.status?.toLowerCase() === 'paid') {
      return 'Processing';
    }
    return order.status;
  }

  getPaymentMethodLabel(order: Order): string {
    const paymentStatus = this.normalizePaymentStatus(order.paymentStatus);
    const paymentMethod = (order.paymentMethod || '').toLowerCase();

    // For seller/admin order details, "Payment Method" should show the actual method first.
    if (paymentMethod === 'cash') return 'ORDER.CASH_ON_DELIVERY';
    if (paymentMethod === 'card') return 'ORDER.ONLINE_PAYMENT';

    // Fallback when legacy/partial data has no method.
    if (paymentStatus === 'failed') return 'ORDER.PAYMENT_FAILED';
    if (paymentStatus === 'unpaid') return 'ORDER.UNPAID';
    if (paymentStatus === 'paid') return 'ORDER.ONLINE_PAYMENT';
    return 'ORDER.UNPAID';
  }

  private normalizePaymentStatus(value: Order['paymentStatus']): string {
    if (typeof value === 'number') {
      if (value === 1) return 'paid';
      if (value === 2) return 'failed';
      return 'unpaid';
    }
    return (value || '').toString().toLowerCase();
  }

  isSellerBlocked(line: any): boolean {
    return !!(line?.sellerIsBlocked || line?.isBlocked);
  }
}

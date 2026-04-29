import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { Order } from '../../../core/models/order.model';

@Component({
  selector: 'app-order-details-modal',
  standalone: true,
  imports: [CommonModule, TranslateModule],
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
    const paymentMethod = (order.paymentMethod || 'Cash').toLowerCase();
    if (paymentMethod === 'cash' && order.status?.toLowerCase() === 'paid') {
      return 'Processing';
    }
    return order.status;
  }

  getPaymentMethodLabel(order: Order): string {
    return (order.paymentMethod || 'Cash') === 'Card'
      ? 'ORDER.ONLINE_PAYMENT'
      : 'ORDER.CASH_ON_DELIVERY';
  }

  isSellerBlocked(line: any): boolean {
    return !!(line?.sellerIsBlocked || line?.isBlocked);
  }
}

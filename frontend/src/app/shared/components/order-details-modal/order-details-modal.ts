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
}

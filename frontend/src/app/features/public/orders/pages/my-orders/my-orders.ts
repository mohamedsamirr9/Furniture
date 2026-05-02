import { OrderService } from '../../../../../core/services/order.service';
import { Order } from '../../../../../core/models/order.model';
import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';


import { TranslateModule } from '@ngx-translate/core';
import { LocalizedPricePipe } from '../../../../../core/pipes/localized-price.pipe';

@Component({
  selector: 'app-my-orders',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, LocalizedPricePipe],
  templateUrl: './my-orders.html',
  styleUrls: ['./my-orders.css'],
   encapsulation: ViewEncapsulation.None
})
export class MyOrdersComponent implements OnInit {
  orders: Order[] = [];
  isLoading = true;
  errorMsg = '';

  constructor(
    private orderService: OrderService
  ) {}

  ngOnInit(): void {
    this.orderService.getMyOrders().subscribe({
      next: (data: any) => {
        // Handle direct array or paginated object structure
        this.orders = Array.isArray(data) ? data : (data.orders || data.items || data.data || []);
        this.isLoading = false;
      },
      error: (err: any) => {
        this.errorMsg = 'Failed to load your orders. Please try again later.';
        this.isLoading = false;
        console.error('Error loading orders', err);
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'pending': return 'bg-warning text-dark';
      case 'processing': return 'bg-info text-dark';
      case 'shipped': return 'bg-primary';
      case 'delivered': 
      case 'completed': return 'bg-success';
      case 'cancelled':
      case 'declined': return 'bg-danger';
      default: return 'bg-secondary';
    }
  }

  getDisplayStatus(order: Order): string {
    const paymentStatus = this.normalizePaymentStatus(order.paymentStatus);
    const paymentMethod = (order.paymentMethod || '').toLowerCase();

    // Backward compatibility: old API used paymentMethod, new API uses paymentStatus
    if ((paymentStatus === 'unpaid' || paymentMethod === 'cash') && order.status?.toLowerCase() === 'paid') {
      return 'Processing';
    }
    return order.status;
  }

  private normalizePaymentStatus(value: Order['paymentStatus']): string {
    if (typeof value === 'number') {
      if (value === 1) return 'paid';
      if (value === 2) return 'failed';
      return 'unpaid';
    }
    return (value || '').toString().toLowerCase();
  }
}

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderService } from '../../../../../core/services/order.service';
import { TranslateModule } from '@ngx-translate/core';
import { Order } from '../../../../../core/models/order.model';
import { OrderDetailsModalComponent } from '../../../../../shared/components/order-details-modal/order-details-modal';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, TranslateModule, OrderDetailsModalComponent],
  templateUrl: './orders.html',
  styleUrl: './orders.css',
})
export class Orders implements OnInit {
  orders: any = [];
  isLoading = true;

  selectedOrder: Order | null = null;
  showOrderDetailsModal = false;
  orderDetailsLoading = false;
  orderDetailsError = '';

  private validTransitions: Record<string, string[]> = {
    'Pending': ['Accepted', 'Declined'],
    'Accepted': ['Paid', 'Cancelled'],
    'Paid': ['Processing'],
    'Processing': ['Shipped'],
    'Shipped': ['Delivered'],
    'Delivered': ['Completed'],
  };

  private terminalStatuses = ['Completed', 'Cancelled', 'Declined'];

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading = true;
    this.orderService.getSellerOrders().subscribe({
      next: (res: any) => {
        this.orders = res;
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Error fetching orders', err);
        this.isLoading = false;
      }
    });
  }

  getValidTransitions(status: string): string[] {
    return this.validTransitions[status] || [];
  }

  isTerminalStatus(status: string): boolean {
    return this.terminalStatuses.includes(status);
  }

  updateStatus(order: any, event: any): void {
    const newStatus = event.target.value;
    const oldStatus = order.status;
    this.orderService.updateOrderStatus(order.id || order.orderId, newStatus).subscribe({
      next: () => {
        order.status = newStatus;
      },
      error: (err: any) => {
        console.error('Error updating order status', err);
        event.target.value = oldStatus;
      }
    });
  }

  viewOrderDetails(orderId: number): void {
    this.showOrderDetailsModal = true;
    this.orderDetailsLoading = true;
    this.orderDetailsError = '';
    this.selectedOrder = null;

    this.orderService.getOrderByIdForSeller(orderId).subscribe({
      next: (order) => {
        this.selectedOrder = order;
        this.orderDetailsLoading = false;
      },
      error: (err: any) => {
        console.error('Error loading order details', err);
        this.orderDetailsError =
          err?.error?.message ?? err?.message ?? 'Failed to load order details';
        this.orderDetailsLoading = false;
      },
    });
  }

  closeOrderDetails(): void {
    this.showOrderDetailsModal = false;
    this.selectedOrder = null;
    this.orderDetailsError = '';
  }
}
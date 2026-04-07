import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderService } from '../../../../../core/services/order.service';

@Component({
  selector: 'app-orders',
  imports: [CommonModule],
  templateUrl: './orders.html',
  styleUrl: './orders.css',
})
export class Orders implements OnInit {
  orders: any = [];
  isLoading = true;

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
    this.orderService.getAllOrdersPaginated(1, 100).subscribe({
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
}
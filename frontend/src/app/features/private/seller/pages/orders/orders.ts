import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { OrderService } from '../../../../../core/services/order.service';
import { SellerService } from '../../../../../core/services/seller.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
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
  private sellerAllowedTransitions = ['Shipped', 'Delivered'];

  statusMessage: string | null = null;
  statusMessageType: 'success' | 'error' = 'success';
  sellerProfile: any;

  constructor(
    private orderService: OrderService,
    private sellerService: SellerService,
    private router: Router,
    private translate: TranslateService
  ) {}

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
    return (this.validTransitions[status] || []).filter(next =>
      this.sellerAllowedTransitions.includes(next)
    );
  }

  isTerminalStatus(status: string): boolean {
    return this.terminalStatuses.includes(status);
  }

  updateStatus(order: any, event: any): void {
    const newStatus = event.target.value;
    const oldStatus = order.status;
    if (!this.getValidTransitions(oldStatus).includes(newStatus)) {
      event.target.value = oldStatus;
      return;
    }
    this.orderService.updateOrderStatus(order.id || order.orderId, newStatus).subscribe({
      next: () => {
        order.status = newStatus;
        
        const orderId = order.id || order.orderId;
        const localizedStatus = this.translate.instant('STATUS.' + newStatus.toUpperCase());
        this.statusMessage = this.translate.instant('ORDER.STATUS_UPDATED', { id: orderId, status: localizedStatus });
        this.statusMessageType = 'success';
        setTimeout(() => this.statusMessage = null, 4000);

        // Refresh financial data silently after delivery
        if (newStatus === 'Delivered') {
          this.sellerService.getMySellerProfile().subscribe(profile => {
            this.sellerProfile = profile;
            setTimeout(() => {
              this.router.navigate(['/seller/payment']);
            }, 1500);
          });
        }
      },
      error: (err: any) => {
        console.error('Error updating order status', err);
        event.target.value = oldStatus;
        this.statusMessage = this.translate.instant('ORDER.STATUS_ERROR');
        this.statusMessageType = 'error';
        setTimeout(() => this.statusMessage = null, 4000);
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
        this.orderDetailsError = this.translate.instant('ORDER.LOAD_DETAILS_ERROR');
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
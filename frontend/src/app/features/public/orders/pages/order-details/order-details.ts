import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { OrderService } from '../../../../../core/services/order.service';
import { Order } from '../../../../../core/models/order.model';

@Component({
  selector: 'app-order-details',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './order-details.html',
  styleUrls: ['./order-details.css']
})
export class OrderDetailsComponent implements OnInit {
  order: Order | null = null;
  isLoading = true;
  errorMsg = '';
  isCancelling = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private orderService: OrderService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadOrder(Number(id));
    } else {
      this.router.navigate(['/orders']);
    }
  }

  loadOrder(id: number): void {
    this.orderService.getOrderById(id).subscribe({
      next: (data) => {
        this.order = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading order', err);
        this.errorMsg = 'Failed to load order details.';
        this.isLoading = false;
      }
    });
  }

  cancelOrder(): void {
    if (!this.order || !confirm("Are you sure you want to cancel this order?")) return;
    
    this.isCancelling = true;
    this.orderService.cancelOrder(this.order.id).subscribe({
      next: () => {
        this.isCancelling = false;
        if(this.order) this.order.status = 'Cancelled';
        alert("Order cancelled successfully");
      },
      error: (err) => {
        this.isCancelling = false;
        console.error(err);
        alert(err.error?.message || "Failed to cancel order.");
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

  get canCancel(): boolean {
    return this.order?.status?.toLowerCase() === 'pending' || this.order?.status?.toLowerCase() === 'processing';
  }
}

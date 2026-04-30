import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { OrderService } from '../../../../../core/services/order.service';
import { ReviewService } from '../../../../../core/services/review.service';
import { Order } from '../../../../../core/models/order.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-order-details',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, TranslateModule],
  templateUrl: './order-details.html',
  styleUrls: ['./order-details.css']
})
export class OrderDetailsComponent implements OnInit {
  order: Order | null = null;
  isLoading = true;
  errorMsg = '';
  isCancelling = false;

  // Review State
  showReviewModal = false;
  selectedProduct: any = null;
  rating = 0;
  reviewMessage = '';
  isSubmittingReview = false;
  reviewedProductIds: Set<number> = new Set();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private orderService: OrderService,
    private reviewService: ReviewService
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
      next: (data: any) => {
        this.order = data;
        this.isLoading = false;
        this.loadUserReviews();
      },
      error: (err: any) => {
        console.error('Error loading order', err);
        this.errorMsg = 'Failed to load order details.';
        this.isLoading = false;
      }
    });
  }

  loadUserReviews(): void {
    this.reviewService.getMyReviewedProductIds().subscribe({
      next: (ids) => {
        this.reviewedProductIds = new Set(ids);
      },
      error: (err) => {
        console.error('Error loading user reviews', err);
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
      error: (err: any) => {
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

  get displayStatus(): string {
    if (!this.order) return '';
    const paymentStatus = (this.order.paymentStatus || '').toLowerCase();
    const paymentMethod = (this.order.paymentMethod || '').toLowerCase();
    if ((paymentStatus === 'unpaid' || paymentMethod === 'cash') && this.order.status?.toLowerCase() === 'paid') {
      return 'Processing';
    }
    return this.order.status;
  }

  get paymentMethodLabel(): string {
    const paymentStatus = (this.order?.paymentStatus || '').toLowerCase();
    const paymentMethod = (this.order?.paymentMethod || '').toLowerCase();
    if (paymentStatus === 'paid') return 'Online Payment';
    if (paymentStatus === 'failed') return 'Payment Failed';
    if (paymentStatus === 'unpaid' && paymentMethod === 'cash') return 'Cash on Delivery';
    if (paymentStatus === 'unpaid' && paymentMethod === 'card') return 'Unpaid';
    if (paymentStatus === 'unpaid') return 'Unpaid';
    if (paymentMethod === 'cash') return 'Cash on Delivery';
    if (paymentMethod === 'card') return 'Online Payment';
    return 'Unpaid';
  }

  get paymentBadgeClass(): string {
    const paymentStatus = (this.order?.paymentStatus || '').toLowerCase();
    const paymentMethod = (this.order?.paymentMethod || '').toLowerCase();
    if (paymentStatus === 'paid') return 'bg-primary';
    if (paymentStatus === 'failed') return 'bg-danger';
    if (paymentStatus === 'unpaid') return 'bg-warning text-dark';
    if (paymentMethod === 'card') return 'bg-primary';
    if (paymentMethod === 'cash') return 'bg-secondary';
    return 'bg-warning text-dark';
  }

  isSellerBlocked(item: any): boolean {
    return !!(item?.sellerIsBlocked || item?.isBlocked);
  }

  // Review Methods
  openReview(item: any): void {
    this.selectedProduct = item;
    this.rating = 0;
    this.reviewMessage = '';
    this.showReviewModal = true;
  }

  closeReview(): void {
    this.showReviewModal = false;
    this.selectedProduct = null;
  }

  setRating(stars: number): void {
    this.rating = stars;
  }

  submitReview(): void {
    if (!this.selectedProduct || this.rating === 0) return;

    this.isSubmittingReview = true;
    this.reviewService.createReview({
      productId: this.selectedProduct.productId,
      rating: this.rating,
      message: this.reviewMessage
    }).subscribe({
      next: () => {
        this.isSubmittingReview = false;
        this.reviewedProductIds.add(this.selectedProduct.productId);
        this.closeReview();
        alert('Thank you for your review!');
      },
      error: (err) => {
        this.isSubmittingReview = false;
        console.error('Review submission failed', err);
        alert(err.error?.message || 'Failed to submit review.');
      }
    });
  }
}

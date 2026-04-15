import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { Order } from '../../../../../core/models/order.model';
import { ComplaintService } from '../../../../../core/services/complaint.service';
import { OrderService } from '../../../../../core/services/order.service';

@Component({
  selector: 'app-new-complaint',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './new-complaint.html',
  styleUrl: './new-complaint.css',
})
export class NewComplaint implements OnInit {
  formData = {
    orderId: '',
    description: '',
  };

  orders: Order[] = [];
  loading = false;
  submitting = false;
  error = '';

  constructor(
    private router: Router,
    private complaintService: ComplaintService,
    private orderService: OrderService,
  ) {}

  ngOnInit() {
    this.loadOrders();
  }

  loadOrders() {
    this.loading = true;
    this.orderService.getMyOrders().subscribe({
      next: (data) => {
        this.orders = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load orders';
        this.loading = false;
        console.error('Error loading orders:', err);
      },
    });
  }

  onSubmit() {
    if (!this.formData.orderId || !this.formData.description) {
      this.error = 'Please fill all fields';
      return;
    }

    this.submitting = true;
    this.error = '';

    const complaintData = {
      orderId: parseInt(this.formData.orderId),
      description: this.formData.description,
    };

    this.complaintService.createComplaint(complaintData).subscribe({
      next: () => {
        this.submitting = false;
        this.router.navigate(['/customer/complaints']);
      },
      error: (err: any) => {
        this.error = 'Failed to submit complaint';
        this.submitting = false;
        console.error('Error submitting complaint:', err);
      },
    });
  }
}

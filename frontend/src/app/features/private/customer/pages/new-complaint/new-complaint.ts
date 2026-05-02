import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { finalize } from 'rxjs';
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
  selectedImageFile: File | null = null;
  imagePreviewUrl = '';

  orders: Order[] = [];
  loading = false;
  submitting = false;
  uploadingImage = false;
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
        this.error = 'COMPLAINTS.ERRORS.LOAD_ORDERS';
        this.loading = false;
        console.error('Error loading orders:', err);
      },
    });
  }

  onSubmit() {
    if (!this.formData.orderId || !this.formData.description) {
      this.error = 'COMPLAINTS.ERRORS.FILL_ALL';
      return;
    }

    if (this.uploadingImage) {
      return;
    }

    this.submitting = true;
    this.error = '';

    if (this.selectedImageFile) {
      this.uploadingImage = true;
      this.complaintService
        .uploadImage(this.selectedImageFile)
        .pipe(finalize(() => (this.uploadingImage = false)))
        .subscribe({
          next: (res) => this.submitComplaint(res.secure_url),
          error: (err: any) => {
            this.error = 'COMPLAINTS.ERRORS.UPLOAD_IMAGE';
            this.submitting = false;
            console.error('Error uploading complaint image:', err);
          },
        });
      return;
    }

    this.submitComplaint();
  }

  onImageSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp', 'image/jpg'];
    if (!allowedTypes.includes(file.type.toLowerCase())) {
      this.error = 'COMPLAINTS.ERRORS.IMAGE_TYPE';
      return;
    }

    const maxSize = 10 * 1024 * 1024;
    if (file.size > maxSize) {
      this.error = 'COMPLAINTS.ERRORS.IMAGE_SIZE';
      return;
    }

    this.error = '';
    this.selectedImageFile = file;
    this.imagePreviewUrl = URL.createObjectURL(file);
  }

  clearImage() {
    this.selectedImageFile = null;
    this.imagePreviewUrl = '';
  }

  private submitComplaint(imageUrl?: string) {
    const complaintData = {
      orderId: parseInt(this.formData.orderId, 10),
      description: this.formData.description,
      imageUrl,
    };

    this.complaintService.createComplaint(complaintData).subscribe({
      next: () => {
        this.submitting = false;
        this.router.navigate(['/customer/complaints']);
      },
      error: (err: any) => {
        this.error = 'COMPLAINTS.ERRORS.SUBMIT';
        this.submitting = false;
        console.error('Error submitting complaint:', err);
      },
    });
  }
}

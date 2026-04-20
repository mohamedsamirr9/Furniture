import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ComplaintDetail, ComplaintStatus } from '../../../core/models/complaint.model';

@Component({
  selector: 'app-complaint-details-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './complaint-details-modal.html',
  styleUrl: './complaint-details-modal.css',
})
export class ComplaintDetailsModalComponent {
  @Input() isOpen = false;
  @Input() loading = false;
  @Input() error = '';
  @Input() complaint: ComplaintDetail | null = null;
  @Input() role: 'admin' | 'seller' | 'buyer' = 'seller';
  @Input() isReplySubmitting = false;
  @Input() isStatusUpdating = false;
  @Output() close = new EventEmitter<void>();
  @Output() submitReply = new EventEmitter<string>();
  @Output() submitStatus = new EventEmitter<ComplaintStatus>();

  replyMessage = '';

  getStatusClass(status?: string): string {
    if (status === 'Open') return 'badge-red';
    if (status === 'InProgress') return 'badge-outline';
    if (status === 'Resolved') return 'badge-gray';
    if (status === 'Closed') return 'badge-green';
    return '';
  }

  getStatusDisplay(status?: string): string {
    if (status === 'InProgress') return 'In Progress';
    return status ?? '';
  }

  get canReply(): boolean {
    return this.role === 'seller' || this.role === 'admin';
  }

  get canUpdateStatus(): boolean {
    return this.role === 'admin';
  }

  get nextStatus(): ComplaintStatus | null {
    if (!this.complaint) return null;
    if (this.complaint.status === ComplaintStatus.Open) return ComplaintStatus.InProgress;
    if (this.complaint.status === ComplaintStatus.InProgress) return ComplaintStatus.Resolved;
    if (this.complaint.status === ComplaintStatus.Resolved) return ComplaintStatus.Closed;
    return null;
  }

  onSubmitReply() {
    const message = this.replyMessage.trim();
    if (!message) return;
    this.submitReply.emit(message);
    this.replyMessage = '';
  }

  onSubmitStatus() {
    if (!this.nextStatus) return;
    this.submitStatus.emit(this.nextStatus);
  }

  onClose() {
    this.close.emit();
  }
}

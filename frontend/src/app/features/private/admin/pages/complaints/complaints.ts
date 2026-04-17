import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Complaint, ComplaintDetail, ComplaintStatus } from '../../../../../core/models/complaint.model';
import { ComplaintService } from '../../../../../core/services/complaint.service';
import { ComplaintDetailsModalComponent } from '../../../../../shared/components/complaint-details-modal/complaint-details-modal';

@Component({
  selector: 'app-complaints',
  standalone: true,
  imports: [CommonModule, TranslateModule, ComplaintDetailsModalComponent],
  templateUrl: './complaints.html',
  styleUrl: './complaints.css',
})
export class Complaints implements OnInit {
  complaints: Complaint[] = [];
  selectedComplaint: ComplaintDetail | null = null;
  showDetailsModal = false;
  detailsLoading = false;
  detailsError = '';
  replySubmitting = false;
  statusUpdating = false;
  loading = false;
  error = '';

  constructor(private complaintService: ComplaintService) {}

  ngOnInit() {
    this.loadComplaints();
  }

  loadComplaints() {
    this.loading = true;
    this.error = '';

    this.complaintService.getAllComplaints().subscribe({
      next: (data: Complaint[]) => {
        this.complaints = data;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to load complaints';
        this.loading = false;
        console.error('Error loading complaints:', err);
      },
    });
  }

  getStatusClass(status: string): string {
    if (status === 'Open') return 'badge-red';
    if (status === 'InProgress') return 'badge-outline';
    if (status === 'Resolved') return 'badge-gray';
    if (status === 'Closed') return 'badge-green';
    return '';
  }

  getStatusDisplay(status: string): string {
    if (status === 'InProgress') return 'In Progress';
    return status;
  }

  submitReply(message: string) {
    if (!this.selectedComplaint) return;
    this.replySubmitting = true;
    this.complaintService.addReply(this.selectedComplaint.id, { message }).subscribe({
      next: () => {
        this.replySubmitting = false;
        this.refreshSelectedComplaint(this.selectedComplaint!.id);
        this.loadComplaints();
      },
      error: (err) => {
        this.replySubmitting = false;
        console.error('Error replying to complaint:', err);
      },
    });
  }

  updateStatus(status: ComplaintStatus) {
    if (!this.selectedComplaint) return;
    this.statusUpdating = true;
    this.complaintService.updateComplaintStatus(this.selectedComplaint.id, { status }).subscribe({
      next: () => {
        this.statusUpdating = false;
        this.refreshSelectedComplaint(this.selectedComplaint!.id);
        this.loadComplaints();
      },
      error: (err) => console.error('Error updating complaint status:', err),
    });
  }

  openComplaintDetails(complaintId: number) {
    this.showDetailsModal = true;
    this.detailsLoading = true;
    this.detailsError = '';
    this.selectedComplaint = null;

    this.complaintService.getComplaintById(complaintId).subscribe({
      next: (detail) => {
        this.selectedComplaint = detail;
        this.detailsLoading = false;
      },
      error: (err) => {
        console.error('Error loading complaint details:', err);
        this.detailsError = 'Failed to load complaint details';
        this.detailsLoading = false;
      },
    });
  }

  closeComplaintDetails() {
    this.showDetailsModal = false;
    this.selectedComplaint = null;
    this.detailsError = '';
    this.replySubmitting = false;
    this.statusUpdating = false;
  }

  private refreshSelectedComplaint(complaintId: number) {
    this.complaintService.getComplaintById(complaintId).subscribe({
      next: (detail) => {
        this.selectedComplaint = detail;
      },
      error: (err) => {
        console.error('Error refreshing complaint details:', err);
      },
    });
  }
}

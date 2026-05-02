import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Complaint, ComplaintDetail } from '../../../../../core/models/complaint.model';
import { ComplaintService } from '../../../../../core/services/complaint.service';
import { TranslateModule } from '@ngx-translate/core';
import { ComplaintDetailsModalComponent } from '../../../../../shared/components/complaint-details-modal/complaint-details-modal';

@Component({
  selector: 'app-complaints',
  standalone: true,
  imports: [CommonModule, ComplaintDetailsModalComponent, TranslateModule],
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
  loading = false;
  error = '';

  constructor(private complaintService: ComplaintService) {}

  ngOnInit() {
    this.loadComplaints();
  }

  loadComplaints() {
    this.loading = true;
    this.error = '';

    this.complaintService.getSellerComplaints().subscribe({
      next: (data: Complaint[]) => {
        this.complaints = data;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'COMPLAINTS.ERRORS.LOAD_LIST';
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
        this.detailsError = 'COMPLAINTS.ERRORS.LOAD_DETAILS';
        this.detailsLoading = false;
      },
    });
  }

  closeComplaintDetails() {
    this.showDetailsModal = false;
    this.selectedComplaint = null;
    this.detailsError = '';
    this.replySubmitting = false;
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

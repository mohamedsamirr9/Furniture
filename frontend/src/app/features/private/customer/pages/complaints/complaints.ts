import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Complaint, ComplaintDetail } from '../../../../../core/models/complaint.model';
import { ComplaintService } from '../../../../../core/services/complaint.service';
import { ComplaintDetailsModalComponent } from '../../../../../shared/components/complaint-details-modal/complaint-details-modal';

@Component({
  selector: 'app-complaints',
  imports: [CommonModule, ComplaintDetailsModalComponent],
  templateUrl: './complaints.html',
  styleUrl: './complaints.css',
})
export class Complaints implements OnInit {
  complaints: Complaint[] = [];
  selectedComplaint: ComplaintDetail | null = null;
  showDetailsModal = false;
  detailsLoading = false;
  detailsError = '';
  loading = false;
  error = '';

  constructor(
    private router: Router,
    private complaintService: ComplaintService,
  ) {}

  ngOnInit() {
    this.loadComplaints();
  }

  loadComplaints() {
    this.loading = true;
    this.error = '';

    this.complaintService.getMyComplaints().subscribe({
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

  newComplaint() {
    this.router.navigate(['/customer/new-complaint']);
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
  }
}

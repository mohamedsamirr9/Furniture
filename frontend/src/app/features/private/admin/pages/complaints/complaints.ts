import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Complaint } from '../../../../../core/models/complaint.model';
import { ComplaintService } from '../../../../../core/services/complaint.service';

@Component({
  selector: 'app-complaints',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './complaints.html',
  styleUrl: './complaints.css',
})
export class Complaints implements OnInit {
  complaints: Complaint[] = [];
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
    if (status === 'Resolved' || status === 'Closed') return 'badge-gray';
    return '';
  }

  getStatusDisplay(status: string): string {
    if (status === 'InProgress') return 'In Progress';
    return status;
  }
}

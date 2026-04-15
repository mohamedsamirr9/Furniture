import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Complaint } from '../../../../../core/models/complaint.model';
import { AuthService } from '../../../../../core/services/auth.service';
import { ComplaintService } from '../../../../../core/services/complaint.service';

@Component({
  selector: 'app-complaints',
  imports: [CommonModule],
  templateUrl: './complaints.html',
  styleUrl: './complaints.css',
})
export class Complaints implements OnInit {
  complaints: Complaint[] = [];
  loading = false;
  error = '';

  constructor(
    private router: Router,
    private complaintService: ComplaintService,
    private authService: AuthService,
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

  getUserId(): string | null {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      try {
        const user = JSON.parse(userStr);
        return user.id || user.userId || user.sub;
      } catch {
        return null;
      }
    }
    return null;
  }

  newComplaint() {
    this.router.navigate(['/customer/new-complaint']);
  }
}

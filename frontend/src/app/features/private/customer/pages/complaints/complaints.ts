import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-complaints',
  imports: [CommonModule],
  templateUrl: './complaints.html',
  styleUrl: './complaints.css',
})
export class Complaints {
  complaints = [
    {
      id: 'C-001',
      description: 'Damaged item received - chair leg was broke',
      date: '2026-03-25',
      status: 'Open',
    },
    {
      id: 'C-001',
      description: 'Damaged item received - chair leg was broke',
      date: '2026-03-25',
      status: 'Resolved',
    },
  ];

  constructor(private router: Router) {}

  newComplaint() {
    this.router.navigate(['/customer/new-complaint']);
  }
}
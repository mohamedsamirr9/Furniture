import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-complaints',
  imports: [CommonModule],
  templateUrl: './complaints.html',
  styleUrl: './complaints.css',
})
export class Complaints {
  complaints = [
    { id: 'C-001', customer: 'Sarah M.', order: 'ORD-1234', description: 'Damaged item received', status: 'Open', date: '2026-03-25' },
    { id: 'C-002', customer: 'James K.', order: 'ORD-1235', description: 'Wrong color delivered', status: 'In Progress', date: '2026-03-22' },
    { id: 'C-003', customer: 'Emily R.', order: 'ORD-1236', description: 'Late delivery', status: 'Resolved', date: '2026-03-18' },
  ];
}
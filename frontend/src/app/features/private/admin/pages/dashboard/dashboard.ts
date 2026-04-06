import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard {
  recentOrders = [
    { id: 'ORD-1234', customer: 'Sarah M.', status: 'Processing' },
    { id: 'ORD-1235', customer: 'James K.', status: 'Shipped' },
  ];

  recentComplaints = [
    { user: 'Lisa P.', description: 'Damaged item received', status: 'Open' },
    { user: 'ORD-1235', description: 'Late delivery - arrived 5 days after', status: 'Resolved' },
  ];
}
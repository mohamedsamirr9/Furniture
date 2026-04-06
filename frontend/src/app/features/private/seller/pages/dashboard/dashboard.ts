import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard {
  stats = [
    { label: 'Total Products', value: '24' },
    { label: 'Active Orders', value: '6' },
    { label: 'Revenue', value: '$2247' },
    { label: 'Open Complaints', value: '1' },
  ];
}
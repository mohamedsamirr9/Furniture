import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, TranslateModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard {
  stats = [
    { label: 'DASHBOARD.TOTAL_PRODUCTS', value: '24' },
    { label: 'DASHBOARD.ACTIVE_ORDERS', value: '6' },
    { label: 'DASHBOARD.REVENUE', value: '$2247' },
    { label: 'DASHBOARD.OPEN_COMPLAINTS', value: '1' },
  ];
}
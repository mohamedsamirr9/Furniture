import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { forkJoin } from 'rxjs';

import { ProductService } from '../../../../../core/services/product.service';
import { OrderService } from '../../../../../core/services/order.service';
import { ComplaintService } from '../../../../../core/services/complaint.service';

const TERMINAL_STATUSES = ['Completed', 'Cancelled', 'Declined'];

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, TranslateModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  isLoading = true;

  stats = [
    { label: 'DASHBOARD.TOTAL_PRODUCTS', value: '—', icon: '📦' },
    { label: 'DASHBOARD.ACTIVE_ORDERS',  value: '—', icon: '🛒' },
    { label: 'DASHBOARD.OPEN_COMPLAINTS', value: '—', icon: '📋' },
  ];

  constructor(
    private productService: ProductService,
    private orderService: OrderService,
    private complaintService: ComplaintService,
  ) {}

  ngOnInit(): void {
    this.loadStats();
  }

  private loadStats(): void {
    this.isLoading = true;

    forkJoin({
      products:   this.productService.getSellerProducts({ page: 1, pageSize: 1 }),
      orders:     this.orderService.getSellerOrders(),
      complaints: this.complaintService.getSellerComplaints(),
    }).subscribe({
      next: ({ products, orders, complaints }) => {
        const totalProducts  = products?.totalCount ?? 0;
        const activeOrders   = (orders ?? []).filter(
          (o: any) => !TERMINAL_STATUSES.includes(o.status)
        ).length;
        const openComplaints = (complaints ?? []).filter(
          (c: any) => c.status === 'Open' || c.status === 'InProgress'
        ).length;

        this.stats = [
          { label: 'DASHBOARD.TOTAL_PRODUCTS',  value: String(totalProducts),  icon: '📦' },
          { label: 'DASHBOARD.ACTIVE_ORDERS',   value: String(activeOrders),   icon: '🛒' },
          { label: 'DASHBOARD.OPEN_COMPLAINTS', value: String(openComplaints), icon: '📋' },
        ];

        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      },
    });
  }
}

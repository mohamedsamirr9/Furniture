import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AuthService } from '../../../../../core/services/auth.service';
import { SearchService } from '../../../../../core/services/search.service';
import { ProductService } from '../../../../../core/services/product.service';
import { OrderService } from '../../../../../core/services/order.service';
import { ComplaintService } from '../../../../../core/services/complaint.service';

declare const bootstrap: any;

const TERMINAL_STATUSES = ['Completed', 'Cancelled', 'Declined'];

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, TranslateModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  @ViewChild('rebuildToast') rebuildToastRef!: ElementRef;

  isRebuilding = false;
  isLoading = true;
  toastMessage = '';
  toastClass = 'text-bg-success';
  private toastInstance: any;

  // Stat cards
  stats = [
    { label: 'ADMIN.TOTAL_USERS',     value: '—' },
    { label: 'ADMIN.TOTAL_PRODUCTS',  value: '—' },
    { label: 'ADMIN.ACTIVE_ORDERS',   value: '—' },
    { label: 'ADMIN.OPEN_COMPLAINTS', value: '—' },
  ];

  // Recent activity lists (bottom grid)
  recentOrders: { id: string; customer: string; status: string }[] = [];
  recentComplaints: { user: string; description: string; status: string }[] = [];

  constructor(
    private authService: AuthService,
    private searchService: SearchService,
    private translate: TranslateService,
    private productService: ProductService,
    private orderService: OrderService,
    private complaintService: ComplaintService,
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  get isAdmin(): boolean {
    return this.authService.getUserRole() === 'admin';
  }

  private loadDashboardData(): void {
    this.isLoading = true;

    forkJoin({
      users:      this.authService.getAllUsers(),
      products:   this.productService.getProducts({ page: 1, pageSize: 1 }),
      orders:     this.orderService.getAllOrdersPaginated(1, 100),
      complaints: this.complaintService.getAllComplaints(),
    }).subscribe({
      next: ({ users, products, orders, complaints }) => {
        // ── Stat cards ──────────────────────────────────────────
        const totalUsers = users?.length ?? 0;
        const totalProducts  = products?.totalCount ?? 0;

        // orders response shape: { data: Order[], totalCount, ... } or Order[]
        const orderList: any[] = Array.isArray(orders) ? orders : (orders?.data ?? []);
        const activeOrders = orderList.filter(
          (o: any) => !TERMINAL_STATUSES.includes(o.status)
        ).length;

        const complaintList: any[] = complaints ?? [];
        const openComplaints = complaintList.filter(
          (c: any) => c.status === 'Open' || c.status === 'InProgress'
        ).length;

        this.stats = [
          { label: 'ADMIN.TOTAL_USERS',     value: String(totalUsers) },
          { label: 'ADMIN.TOTAL_PRODUCTS',  value: String(totalProducts) },
          { label: 'ADMIN.ACTIVE_ORDERS',   value: String(activeOrders) },
          { label: 'ADMIN.OPEN_COMPLAINTS', value: String(openComplaints) },
        ];

        // ── Recent Orders (latest 5) ─────────────────────────────
        this.recentOrders = [...orderList]
          .sort((a: any, b: any) =>
            new Date(b.createdAt ?? b.orderDate ?? 0).getTime() -
            new Date(a.createdAt ?? a.orderDate ?? 0).getTime()
          )
          .slice(0, 5)
          .map((o: any) => ({
            id:       `ORD-${o.id ?? o.orderId}`,
            customer: o.userName ?? o.buyerName ?? '—',
            status:   o.status ?? 'Pending',
          }));

        // ── Recent Complaints (latest 3 Open/InProgress) ─────────
        this.recentComplaints = [...complaintList]
          .sort((a: any, b: any) =>
            new Date(b.createdAt ?? 0).getTime() -
            new Date(a.createdAt ?? 0).getTime()
          )
          .slice(0, 3)
          .map((c: any) => ({
            user:        c.userName ?? c.userId ?? '—',
            description: c.description ?? '',
            status:      c.status ?? 'Open',
          }));

        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      },
    });
  }

  rebuildSearchIndex(): void {
    if (!this.isAdmin || this.isRebuilding) return;

    const confirmed = confirm(this.translate.instant('SEARCH.REBUILD_CONFIRM'));
    if (!confirmed) return;

    this.isRebuilding = true;
    this.searchService
      .rebuildIndex()
      .pipe(finalize(() => (this.isRebuilding = false)))
      .subscribe({
        next: () => this.showToast('SEARCH.REBUILD_SUCCESS', 'text-bg-success'),
        error: () => this.showToast('SEARCH.REBUILD_ERROR', 'text-bg-danger'),
      });
  }

  private showToast(messageKey: string, toastClass: string): void {
    this.toastMessage = this.translate.instant(messageKey);
    this.toastClass = toastClass;
    if (!this.rebuildToastRef) return;
    const toastElement = this.rebuildToastRef.nativeElement;
    this.toastInstance = this.toastInstance || new bootstrap.Toast(toastElement, { delay: 3500 });
    this.toastInstance.show();
  }
}
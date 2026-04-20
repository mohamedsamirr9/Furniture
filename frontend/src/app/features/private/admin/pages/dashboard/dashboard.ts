import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AuthService } from '../../../../../core/services/auth.service';
import { SearchService } from '../../../../../core/services/search.service';

declare const bootstrap: any;

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, TranslateModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  @ViewChild('rebuildToast') rebuildToastRef!: ElementRef;

  isRebuilding = false;
  toastMessage = '';
  toastClass = 'text-bg-success';
  private toastInstance: any;

  recentOrders = [
    { id: 'ORD-1234', customer: 'Sarah M.', status: 'Processing' },
    { id: 'ORD-1235', customer: 'James K.', status: 'Shipped' },
  ];

  recentComplaints = [
    { user: 'Lisa P.', description: 'Damaged item received', status: 'Open' },
    { user: 'ORD-1235', description: 'Late delivery - arrived 5 days after', status: 'Resolved' },
  ];

  constructor(
    private authService: AuthService,
    private searchService: SearchService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {}

  get isAdmin(): boolean {
    return this.authService.getUserRole() === 'admin';
  }

  rebuildSearchIndex(): void {
    if (!this.isAdmin || this.isRebuilding) {
      return;
    }

    const confirmed = confirm(this.translate.instant('SEARCH.REBUILD_CONFIRM'));
    if (!confirmed) {
      return;
    }

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

    if (!this.rebuildToastRef) {
      return;
    }

    const toastElement = this.rebuildToastRef.nativeElement;
    this.toastInstance = this.toastInstance || new bootstrap.Toast(toastElement, { delay: 3500 });
    this.toastInstance.show();
  }
}
import { Component, OnInit, HostListener } from '@angular/core';
import { NotificationService } from '../../../../../core/services/notification.service';
import { Notification } from '../../../../../core/models/notification.model';
import { AsyncPipe, DatePipe, NgFor, NgIf } from '@angular/common';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-notification',
  templateUrl: './notification.html',
  styleUrls: ['./notification.css'],
  imports: [AsyncPipe, DatePipe, NgFor, NgIf, TranslateModule]
})
export class NotificationComponent implements OnInit {
  private static readonly titleKeys: Record<string, string> = {
    'New Custom Request': 'NOTIFICATIONS.NEW_CUSTOM_REQUEST_TITLE',
    'Custom Request Updated': 'NOTIFICATIONS.CUSTOM_REQUEST_UPDATED_TITLE',
  };

  notifications$;
  unreadCount$;
  isOpen = false;

  constructor(
    private notificationService: NotificationService,
    private router: Router
  ) {
    this.notifications$ = this.notificationService.notifications$;
    this.unreadCount$ = this.notificationService.unreadCount$;
  }

  ngOnInit(): void {
    this.notificationService.loadNotifications();
  }

  toggleDropdown(): void {
    this.isOpen = !this.isOpen;
  }

  @HostListener('document:click')
  closeDropdown(): void {
    this.isOpen = false;
  }

  /** Maps API title strings to i18n keys (backend sends fixed English titles). */
  notificationTitleKey(title: string): string {
    return NotificationComponent.titleKeys[title] ?? title;
  }

  /**
   * Maps API message patterns to i18n keys + params (backend uses English templates).
   */
  notificationMessageI18n(
    message: string | undefined
  ): { key: string; params?: Record<string, string> } {
    const msg = (message ?? '').trim();
    const created = /^New Custom Request Has Been Added:\s*(.*)$/i.exec(msg);
    if (created) {
      const description = (created[1] ?? '').trim();
      if (description) {
        return {
          key: 'NOTIFICATIONS.NEW_CUSTOM_REQUEST_ADDED_WITH_DESC',
          params: { description },
        };
      }
      return { key: 'NOTIFICATIONS.NEW_CUSTOM_REQUEST_ADDED' };
    }
    const updated = /^A custom request has been updated:\s*(.*)$/i.exec(msg);
    if (updated) {
      const description = (updated[1] ?? '').trim();
      return {
        key: 'NOTIFICATIONS.CUSTOM_REQUEST_UPDATED_WITH_DESC',
        params: { description },
      };
    }
    return { key: 'NOTIFICATIONS.GENERIC_MESSAGE', params: { text: msg } };
  }

  onRead(notification: Notification): void {
    const navigateToTarget = () => {
      this.isOpen = false;
      this.router.navigate(['/seller/offers']);
    };

    if (notification.isRead) {
      navigateToTarget();
      return;
    }

    this.notificationService.markAsReadLocally(notification.id);

    this.notificationService.markAsRead(notification.id).subscribe({
      next: () => {
        this.notificationService.loadNotifications();
        navigateToTarget();
      },
      error: (err) => {
        console.error('mark as read failed', err);
        this.notificationService.loadNotifications();
        navigateToTarget();
      }
    });
  }
}
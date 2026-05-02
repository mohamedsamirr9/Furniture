import { Component, OnInit, HostListener } from '@angular/core';
import { NotificationService } from '../../../../../core/services/notification.service';
import { Notification } from '../../../../../core/models/notification.model';
import { AsyncPipe, DatePipe, NgFor, NgIf } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-notification',
  templateUrl: './notification.html',
  styleUrls: ['./notification.css'],
  imports: [AsyncPipe, DatePipe, NgFor, NgIf]
})
export class NotificationComponent implements OnInit {
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

  onRead(notification: Notification): void {
    const navigateToTarget = () => {
      if (notification.customRequestId) {
        this.isOpen = false;
        this.router.navigate(['/custom-requests', notification.customRequestId]);
      }
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
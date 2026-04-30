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
    const token = localStorage.getItem('token');
    if (token) {
      this.notificationService.startConnection(token);
    }
    this.notificationService.loadNotifications();
  }

  toggleDropdown(): void {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.notificationService.loadNotifications();
    }
  }

  @HostListener('document:click')
  closeDropdown(): void {
    this.isOpen = false;
  }

  onRead(notification: Notification): void {
    if (!notification.isRead) {
      this.notificationService.markAsReadLocally(notification.id);

      this.notificationService.markAsRead(notification.id).subscribe({
        error: (err) => console.error('mark as read failed', err)
      });
    }

    if (notification.customRequestId) {
      this.isOpen = false;
      setTimeout(() => {
        this.router.navigate(['/custom-requests', notification.customRequestId]);
      }, 100);
    }
  }
}
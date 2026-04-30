import { Component, OnInit } from '@angular/core';
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

  onRead(notification: Notification): void {
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe();
      this.notificationService.markAsReadLocally(notification.id);
    }

    if (notification.customRequestId) {
      this.isOpen = false;
      this.router.navigate(['/custom-requests', notification.customRequestId]);
    }
  }
}
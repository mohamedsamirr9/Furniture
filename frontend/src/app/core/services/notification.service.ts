import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Notification } from '../models/notification.model';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private hubConnection!: signalR.HubConnection;

  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  notifications$ = this.notificationsSubject.asObservable();

  private unreadCountSubject = new BehaviorSubject<number>(0);
  unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(private http: HttpClient) {}

  startConnection(token: string): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/notificationHub`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch(err => console.error('SignalR error:', err));

    this.hubConnection.on('ReceiveNotification', (notification: Notification) => {
      const current = this.notificationsSubject.value;
      this.notificationsSubject.next([notification, ...current]);
      this.unreadCountSubject.next(this.unreadCountSubject.value + 1);
    });
  }

  stopConnection(): void {
    this.hubConnection?.stop();
  }

  getMyNotifications() {
    return this.http.get<Notification[]>(`${environment.apiUrl}/api/notifications`);
  }

  markAsRead(id: number) {
    return this.http.patch(`${environment.apiUrl}/api/notifications/${id}/read`, {});
  }

  loadNotifications(): void {
    this.getMyNotifications().subscribe(notifications => {
      this.notificationsSubject.next(notifications);
      const unread = notifications.filter(n => !n.isRead).length;
      this.unreadCountSubject.next(unread);
    });
  }

  markAsReadLocally(id: number): void {
    const updated = this.notificationsSubject.value.map(n =>
      n.id === id ? { ...n, isRead: true } : n
    );
    this.notificationsSubject.next(updated);
    this.unreadCountSubject.next(updated.filter(n => !n.isRead).length);
  }
}
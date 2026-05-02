import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Notification } from '../models/notification.model';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private hubConnection?: signalR.HubConnection;

  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  notifications$ = this.notificationsSubject.asObservable();

  private unreadCountSubject = new BehaviorSubject<number>(0);
  unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(private http: HttpClient) {}

  private getStorageKey(): string | null {
    const userData = localStorage.getItem('user');
    if (!userData) return null;

    const user = JSON.parse(userData);
    return user?.id ? `notifications_cache_${user.id}` : null;
  }

  restoreFromStorage(): void {
    const key = this.getStorageKey();
    if (!key) {
      this.clearNotificationsState();
      return;
    }

    const data = localStorage.getItem(key);
    const notifications: Notification[] = data ? JSON.parse(data) : [];
    this.updateState(notifications, false);
  }

  private updateState(notifications: Notification[], persist = true): void {
    this.notificationsSubject.next(notifications);
    this.unreadCountSubject.next(notifications.filter(n => !n.isRead).length);

    if (persist) {
      const key = this.getStorageKey();
      if (key) {
        localStorage.setItem(key, JSON.stringify(notifications));
      }
    }
  }

  async startConnection(token: string): Promise<void> {
    await this.stopConnection();

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/notificationHub`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveNotification', (notification: Notification) => {
      const current = this.notificationsSubject.value;
      const updated = [notification, ...current.filter(n => n.id !== notification.id)];
      this.updateState(updated);
    });

    try {
      await this.hubConnection.start();
      console.log('SignalR connected');
    } catch (err) {
      console.error('SignalR connection error:', err);
    }
  }

  async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      this.hubConnection.off('ReceiveNotification');
      await this.hubConnection.stop();
      this.hubConnection = undefined;
    }
  }

  getMyNotifications() {
    return this.http.get<Notification[]>(`${environment.apiUrl}/api/notifications`);
  }

  markAsRead(id: number) {
    return this.http.patch(`${environment.apiUrl}/api/notifications/${id}/read`, {});
  }

  loadNotifications(): void {
    this.getMyNotifications().subscribe({
      next: (notifications) => {
        this.updateState(notifications);
      },
      error: (err) => {
        console.error('Load notifications failed:', err);
      }
    });
  }

  markAsReadLocally(id: number): void {
    const current = this.notificationsSubject.value;
    const updated = current.map(n =>
      n.id === id ? { ...n, isRead: true } : n
    );
    this.updateState(updated);
  }

  clearNotificationsState(): void {
    this.notificationsSubject.next([]);
    this.unreadCountSubject.next(0);
  }
}
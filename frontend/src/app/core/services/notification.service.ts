import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Notification } from '../models/notification.model';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private hubConnection?: signalR.HubConnection;
  private readonly storageKey = 'notifications_cache';

  private initialNotifications = this.getNotificationsFromStorage();

  private notificationsSubject = new BehaviorSubject<Notification[]>(this.initialNotifications);
  notifications$ = this.notificationsSubject.asObservable();

  private unreadCountSubject = new BehaviorSubject<number>(
    this.initialNotifications.filter(n => !n.isRead).length
  );
  unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(private http: HttpClient) {}

  private getNotificationsFromStorage(): Notification[] {
    const data = localStorage.getItem(this.storageKey);
    return data ? JSON.parse(data) : [];
  }

  private updateState(notifications: Notification[]): void {
    this.notificationsSubject.next(notifications);
    this.unreadCountSubject.next(notifications.filter(n => !n.isRead).length);
    localStorage.setItem(this.storageKey, JSON.stringify(notifications));
  }

  startConnection(token: string): void {
    if (this.hubConnection && this.hubConnection.state !== signalR.HubConnectionState.Disconnected) {
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/notificationHub`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveNotification', (notification: Notification) => {
      console.log('Realtime notification:', notification);
      const current = this.notificationsSubject.value;
      const updated = [notification, ...current.filter(n => n.id !== notification.id)];
      this.updateState(updated);
    });

    this.hubConnection.start()
      .then(() => console.log('SignalR connected'))
      .catch(err => console.error('SignalR error:', err));
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
    this.getMyNotifications().subscribe({
      next: (notifications) => {
        console.log('Notifications from API:', notifications);
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
    this.updateState([]);
    localStorage.removeItem(this.storageKey);
  }
}
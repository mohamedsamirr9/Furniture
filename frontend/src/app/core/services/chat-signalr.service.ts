import { Injectable, OnDestroy } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, Subject, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import {
  ConversationDto,
  MessageDto,
  SendMessageDto,
  StartConversationDto,
} from '../models/chat.model';

@Injectable({
  providedIn: 'root',
})
export class ChatSignalRService implements OnDestroy {
  private hubConnection: HubConnection | undefined;
  private baseUrl = `${environment.apiUrl}/Chat`;
  private hubUrl = `${environment.apiUrl}/chatHub`;

  private conversationsSubject = new BehaviorSubject<ConversationDto[]>([]);
  public conversations$ = this.conversationsSubject.asObservable();

  private messageReceivedSubject = new Subject<MessageDto>();
  public messageReceived$ = this.messageReceivedSubject.asObservable();

  private messagesReadSubject = new Subject<{ conversationId: number; userId: string }>();
  public messagesRead$ = this.messagesReadSubject.asObservable();

  private userTypingSubject = new Subject<{ conversationId: number; userId: string }>();
  public userTyping$ = this.userTypingSubject.asObservable();

  private connectionEstablishedSubject = new BehaviorSubject<boolean>(false);
  public connectionEstablished$ = this.connectionEstablishedSubject.asObservable();

  private activeConversations = new Set<number>();

  constructor(
    private http: HttpClient,
    private authService: AuthService,
  ) {}

  public startConnection(): void {
    if (this.hubConnection) {
      return;
    }

    const token = this.authService.token;
    if (!token) {
      console.error('No token available for SignalR connection');
      return;
    }

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Information)
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR connection established');
        this.connectionEstablishedSubject.next(true);
      })
      .catch((err) => {
        console.error('Error establishing SignalR connection:', err);
        this.connectionEstablishedSubject.next(false);
      });

    this.registerOnServerEvents();
  }

  public stopConnection(): void {
    if (this.hubConnection) {
      this.activeConversations.forEach((convId) => {
        this.leaveConversation(convId);
      });
      this.hubConnection.stop();
      this.hubConnection = undefined;
      this.connectionEstablishedSubject.next(false);
    }
  }

  private registerOnServerEvents(): void {
    if (!this.hubConnection) return;

    // Remove any existing handlers first to prevent duplicates
    this.hubConnection.off('ReceiveMessage');
    this.hubConnection.off('MessagesRead');
    this.hubConnection.off('UserTyping');
    this.hubConnection.off('JoinedConversation');

    this.hubConnection.on('ReceiveMessage', (message: MessageDto) => {
      this.messageReceivedSubject.next(message);
      this.updateConversationLastMessage(message);
    });

    this.hubConnection.on('MessagesRead', (conversationId: number, userId: string) => {
      this.messagesReadSubject.next({ conversationId, userId });
      this.updateConversationReadStatus(conversationId, userId);
    });

    this.hubConnection.on('UserTyping', (conversationId: number, userId: string) => {
      this.userTypingSubject.next({ conversationId, userId });
    });

    this.hubConnection.on('JoinedConversation', (conversationId: number) => {
      console.log(`Joined conversation ${conversationId}`);
    });
  }

  public joinConversation(conversationId: number): void {
    if (!this.hubConnection || this.activeConversations.has(conversationId)) return;

    this.hubConnection
      .invoke('JoinConversation', conversationId)
      .then(() => {
        this.activeConversations.add(conversationId);
      })
      .catch((err) => console.error('Error joining conversation:', err));
  }

  public leaveConversation(conversationId: number): void {
    if (!this.hubConnection || !this.activeConversations.has(conversationId)) return;

    this.hubConnection
      .invoke('LeaveConversation', conversationId)
      .then(() => {
        this.activeConversations.delete(conversationId);
      })
      .catch((err) => console.error('Error leaving conversation:', err));
  }

  public sendMessage(conversationId: number, content: string): void {
    if (!this.hubConnection) return;

    this.hubConnection
      .invoke('SendMessage', conversationId, content)
      .catch((err) => console.error('Error sending message:', err));
  }

  public markAsRead(conversationId: number): void {
    if (!this.hubConnection) return;

    this.hubConnection
      .invoke('MarkAsRead', conversationId)
      .catch((err) => console.error('Error marking as read:', err));
  }

  public sendTypingIndicator(conversationId: number): void {
    if (!this.hubConnection) return;

    this.hubConnection
      .invoke('TypingIndicator', conversationId)
      .catch((err) => console.error('Error sending typing indicator:', err));
  }

  public getConversations(): Observable<ConversationDto[]> {
    return this.http
      .get<ConversationDto[]>(`${this.baseUrl}/conversations`)
      .pipe(tap((conversations) => this.conversationsSubject.next(conversations)));
  }

  public getMessages(conversationId: number): Observable<MessageDto[]> {
    return this.http.get<MessageDto[]>(`${this.baseUrl}/conversations/${conversationId}/messages`);
  }

  public startConversation(dto: StartConversationDto): Observable<ConversationDto> {
    return this.http.post<ConversationDto>(`${this.baseUrl}/conversations`, dto).pipe(
      tap((conversation) => {
        const current = this.conversationsSubject.value;
        this.conversationsSubject.next([conversation, ...current]);
      }),
    );
  }

  public sendMessageRest(dto: SendMessageDto): Observable<MessageDto> {
    return this.http.post<MessageDto>(`${this.baseUrl}/messages`, dto);
  }

  public markAsReadRest(conversationId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/conversations/${conversationId}/read`, {});
  }

  private updateConversationLastMessage(message: MessageDto): void {
    const conversations = this.conversationsSubject.value;
    const index = conversations.findIndex((c) => c.id === message.conversationId);
    if (index !== -1) {
      conversations[index].lastMessage = message;
      conversations[index].unreadCount =
        message.senderId !== this.authService.token ? conversations[index].unreadCount + 1 : 0;
      this.conversationsSubject.next([...conversations]);
    }
  }

  private updateConversationReadStatus(conversationId: number, userId: string): void {
    const conversations = this.conversationsSubject.value;
    const index = conversations.findIndex((c) => c.id === conversationId);
    if (
      (index !== -1 && conversations[index].customerId === userId) ||
      conversations[index].sellerId === userId
    ) {
      this.conversationsSubject.next([...conversations]);
    }
  }

  public getUnreadCount(): number {
    return this.conversationsSubject.value.reduce((sum, c) => sum + c.unreadCount, 0);
  }

  ngOnDestroy(): void {
    this.stopConnection();
  }
}

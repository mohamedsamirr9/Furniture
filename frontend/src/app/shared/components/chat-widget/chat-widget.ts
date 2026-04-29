import { Component, OnInit, OnDestroy, HostListener, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChatSignalRService } from '../../../core/services/chat-signalr.service';
import { ConversationDto } from '../../../core/models/chat.model';
import { ChatListPanelComponent } from './chat-list-panel/chat-list-panel';
import { ChatBoxComponent } from './chat-box/chat-box';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-chat-widget',
  standalone: true,
  imports: [CommonModule, ChatListPanelComponent, ChatBoxComponent],
  template: `
    @if (showChatPanel) {
      <app-chat-list-panel
        (conversationSelected)="onConversationSelected($event)"
        (closePanelEvent)="onClosePanel()"
      ></app-chat-list-panel>
    }

    @for (conv of openConversations; track conv.id; let i = $index) {
      <app-chat-box
        [conversation]="conv"
        [position]="i"
        (closeChatBoxEvent)="onCloseChatBox($event)"
      ></app-chat-box>
    }

    <div class="chat-icon" [class.has-unread]="unreadCount > 0" (click)="toggleChatPanel()">
      <svg
        xmlns="http://www.w3.org/2000/svg"
        width="24"
        height="24"
        viewBox="0 0 24 24"
        fill="white"
      >
        <path
          d="M20 2H4c-1.1 0-2 .9-2 2v18l4-4h14c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm0 14H6l-2 2V4h16v12z"
        />
        <path d="M7 9h10v2H7zm0-3h10v2H7zm0 6h7v2H7z" />
      </svg>
      @if (unreadCount > 0) {
        <span class="unread-badge">{{ unreadCount > 99 ? '99+' : unreadCount }}</span>
      }
    </div>
  `,
  styles: [
    `
      .chat-icon {
        position: fixed;
        bottom: 20px;
        right: 20px;
        width: 60px;
        height: 60px;
        border-radius: 50%;
        background: #1877f2;
        display: flex;
        align-items: center;
        justify-content: center;
        cursor: pointer;
        box-shadow: 0 2px 12px rgba(0, 0, 0, 0.2);
        z-index: 998;
        transition: background 0.3s;
      }

      .chat-icon:hover {
        background: #166fe5;
      }

      .chat-icon.has-unread {
        background: #42b72a;
      }

      .unread-badge {
        position: absolute;
        top: -5px;
        right: -5px;
        background: #ff4444;
        color: white;
        border-radius: 50%;
        width: 22px;
        height: 22px;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 11px;
        font-weight: bold;
        border: 2px solid white;
      }
    `,
  ],
})
export class ChatWidgetComponent implements OnInit, OnDestroy {
  showChatPanel = false;
  openConversations: ConversationDto[] = [];
  unreadCount = 0;
  private destroy$ = new Subject<void>();

  constructor(
    private chatService: ChatSignalRService,
    private ngZone: NgZone,
  ) {}

  ngOnInit(): void {
    // Start SignalR connection
    this.chatService.startConnection();

    // Listen for conversation open events from other components
    window.addEventListener(
      'openConversation',
      this.handleOpenConversation.bind(this) as EventListener,
    );

    // Listen for real-time message updates to update unread count
    this.chatService.messageReceived$.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.updateUnreadCount();
    });

    this.chatService.conversations$.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.updateUnreadCount();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    window.removeEventListener('openConversation', this.handleOpenConversation as EventListener);
    this.chatService.stopConnection();
  }

  toggleChatPanel(): void {
    this.showChatPanel = !this.showChatPanel;
    if (this.showChatPanel) {
      this.chatService.getConversations().subscribe();
    }
  }

  onCloseChatBox(conversationId: number): void {
    this.openConversations = this.openConversations.filter((c) => c.id !== conversationId);
    // Adjust positions of remaining chat boxes
  }

  onConversationSelected(conversation: ConversationDto): void {
    const exists = this.openConversations.find((c) => c.id === conversation.id);
    if (!exists) {
      this.showChatPanel = false;
      this.openConversations.push(conversation);
      this.chatService.markAsRead(conversation.id);
    }
  }

  private handleOpenConversation(event: CustomEvent): void {
    this.ngZone.run(() => {
      const conversation = event.detail as ConversationDto;
      const exists = this.openConversations.find((c) => c.id === conversation.id);
      if (!exists) {
        this.showChatPanel = false;
        this.openConversations.push(conversation);
        this.chatService.markAsRead(conversation.id);
      }
    });
  }

  onClosePanel(): void {
    this.ngZone.run(() => {
      this.showChatPanel = false;
    });
  }

  private updateUnreadCount(): void {
    this.unreadCount = this.chatService.getUnreadCount();
  }
}

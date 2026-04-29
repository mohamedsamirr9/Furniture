import { Component, OnInit, OnDestroy, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatSignalRService } from '../../../../core/services/chat-signalr.service';
import { ConversationDto } from '../../../../core/models/chat.model';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-chat-list-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="chat-list-panel">
      <div class="chat-list-header">
        <h3>Messages</h3>
        <button class="close-btn" (click)="closePanel()">×</button>
      </div>
      <div class="chat-list-search">
        <input type="text" placeholder="Search conversations..." [(ngModel)]="searchTerm" (ngModelChange)="filterConversations()"/>
      </div>
      <div class="chat-list-items">
        @for (conv of filteredConversations; track conv.id) {
          <div class="conversation-item" [class.unread]="conv.unreadCount > 0" (click)="openConversation(conv)">
            <div class="conversation-avatar">
              {{ getOtherUserName(conv)[0] }}
            </div>
            <div class="conversation-info">
              <div class="conversation-name">{{ getOtherUserName(conv) }}</div>
              <div class="conversation-last-message">
                @if (conv.lastMessage) {
                  {{ conv.lastMessage.content | slice:0:30 }}{{ conv.lastMessage.content.length > 30 ? '...' : '' }}
                } @else {
                  No messages yet
                }
              </div>
            </div>
            <div class="conversation-meta">
              @if (conv.unreadCount > 0) {
                <span class="unread-badge">{{ conv.unreadCount }}</span>
              }
              @if (conv.lastMessage) {
                <span class="time">{{ conv.lastMessage.sentAt | date:'shortTime' }}</span>
              }
            </div>
          </div>
        } @empty {
          <div class="no-conversations">No conversations yet</div>
        }
      </div>
    </div>
  `,
  styles: [`
    .chat-list-panel {
      position: fixed;
      bottom: 60px;
      right: 20px;
      width: 350px;
      height: 500px;
      background: white;
      border-radius: 10px 10px 0 0;
      box-shadow: 0 5px 40px rgba(0,0,0,0.16);
      display: flex;
      flex-direction: column;
      z-index: 999;
    }

    .chat-list-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 15px;
      border-bottom: 1px solid #e0e0e0;
      background: #1877f2;
      color: white;
      border-radius: 10px 10px 0 0;
    }

    .chat-list-header h3 {
      margin: 0;
      font-size: 16px;
    }

    .close-btn {
      background: none;
      border: none;
      color: white;
      font-size: 24px;
      cursor: pointer;
      padding: 0;
      line-height: 1;
    }

    .chat-list-search {
      padding: 10px;
      border-bottom: 1px solid #e0e0e0;
    }

    .chat-list-search input {
      width: 100%;
      padding: 8px 12px;
      border: 1px solid #e0e0e0;
      border-radius: 20px;
      outline: none;
      font-size: 14px;
    }

    .chat-list-items {
      flex: 1;
      overflow-y: auto;
    }

    .conversation-item {
      display: flex;
      align-items: center;
      padding: 12px 15px;
      cursor: pointer;
      transition: background 0.2s;
      border-bottom: 1px solid #f0f0f0;
    }

    .conversation-item:hover {
      background: #f5f5f5;
    }

    .conversation-item.unread {
      background: #e7f3ff;
    }

    .conversation-avatar {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      background: #1877f2;
      color: white;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: bold;
      margin-right: 12px;
      flex-shrink: 0;
    }

    .conversation-info {
      flex: 1;
      min-width: 0;
    }

    .conversation-name {
      font-weight: 600;
      font-size: 14px;
      margin-bottom: 4px;
    }

    .conversation-last-message {
      font-size: 13px;
      color: #65676b;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .conversation-meta {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      margin-left: 8px;
    }

    .unread-badge {
      background: #1877f2;
      color: white;
      border-radius: 50%;
      width: 20px;
      height: 20px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 11px;
      font-weight: bold;
    }

    .time {
      font-size: 11px;
      color: #65676b;
      margin-top: 4px;
    }

    .no-conversations {
      text-align: center;
      padding: 40px 20px;
      color: #65676b;
      font-size: 14px;
    }
  `]
})
export class ChatListPanelComponent implements OnInit, OnDestroy {
  @Output() conversationSelected = new EventEmitter<ConversationDto>();
  conversations: ConversationDto[] = [];
  filteredConversations: ConversationDto[] = [];
  searchTerm: string = '';
  private destroy$ = new Subject<void>();

  constructor(private chatService: ChatSignalRService) {}

  ngOnInit(): void {
    this.chatService.conversations$
      .pipe(takeUntil(this.destroy$))
      .subscribe(conversations => {
        this.conversations = conversations;
        this.filterConversations();
      });

    this.chatService.getConversations().subscribe();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  getOtherUserName(conv: ConversationDto): string {
    const currentUserId = this.getCurrentUserId();
    return currentUserId === conv.customerId ? conv.sellerName : conv.customerName;
  }

  filterConversations(): void {
    if (!this.searchTerm) {
      this.filteredConversations = this.conversations;
    } else {
      const term = this.searchTerm.toLowerCase();
      this.filteredConversations = this.conversations.filter(conv =>
        this.getOtherUserName(conv).toLowerCase().includes(term)
      );
    }
  }

  openConversation(conv: ConversationDto): void {
    this.conversationSelected.emit(conv);
  }

  closePanel(): void {
    this.closePanelEvent.emit();
  }
  
  @Output() closePanelEvent = new EventEmitter<void>();

  private getCurrentUserId(): string {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      const user = JSON.parse(userStr);
      return user.id || '';
    }
    return '';
  }
}

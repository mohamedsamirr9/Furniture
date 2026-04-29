import {
  Component,
  OnInit,
  OnDestroy,
  Input,
  ElementRef,
  ViewChild,
  Output,
  EventEmitter,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatSignalRService } from '../../../../core/services/chat-signalr.service';
import { ConversationDto, MessageDto } from '../../../../core/models/chat.model';
import { Subject, takeUntil, tap } from 'rxjs';

@Component({
  selector: 'app-chat-box',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="chat-box" [class.minimized]="isMinimized">
      <div class="chat-box-header" (click)="toggleMinimize()">
        <div class="header-info">
          <div class="avatar">{{ otherUserName[0] }}</div>
          <span class="user-name">{{ otherUserName }}</span>
        </div>
        <div class="header-actions">
          <button class="action-btn" (click)="closeChat($event)">×</button>
        </div>
      </div>

      @if (!isMinimized) {
        <div class="chat-box-messages" #messageContainer>
          @for (msg of messages; track msg.id) {
            <div class="message" [class.own-message]="isOwnMessage(msg)">
              <div class="message-content">{{ msg.content }}</div>
              <div class="message-time">{{ msg.sentAt | date: 'shortTime' }}</div>
            </div>
          }
          @if (typingUser) {
            <div class="typing-indicator">User is typing...</div>
          }
        </div>

        <div class="chat-box-input">
          <input
            type="text"
            placeholder="Type a message..."
            [(ngModel)]="newMessage"
            (keyup.enter)="sendMessage()"
            (input)="onTyping()"
          />
          <button (click)="sendMessage()">Send</button>
        </div>
      }
    </div>
  `,
  styles: [
    `
      .chat-box {
        position: fixed;
        bottom: 60px;
        width: 300px;
        height: 400px;
        background: white;
        border-radius: 10px 10px 0 0;
        box-shadow: 0 5px 40px rgba(0, 0, 0, 0.16);
        display: flex;
        flex-direction: column;
        z-index: 1000;
      }

      .chat-box.minimized {
        height: auto;
      }

      .chat-box-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 10px 15px;
        background: #1877f2;
        color: white;
        border-radius: 10px 10px 0 0;
        cursor: pointer;
      }

      .header-info {
        display: flex;
        align-items: center;
        gap: 10px;
      }

      .avatar {
        width: 32px;
        height: 32px;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.2);
        display: flex;
        align-items: center;
        justify-content: center;
        font-weight: bold;
      }

      .user-name {
        font-weight: 600;
        font-size: 14px;
      }

      .header-actions {
        display: flex;
        gap: 5px;
      }

      .action-btn {
        background: none;
        border: none;
        color: white;
        font-size: 18px;
        cursor: pointer;
        padding: 0 5px;
        line-height: 1;
      }

      .chat-box-messages {
        flex: 1;
        overflow-y: auto;
        padding: 15px;
        background: #f0f2f5;
      }

      .message {
        margin-bottom: 10px;
        display: flex;
        flex-direction: column;
      }

      .message.own-message {
        align-items: flex-end;
      }

      .message-content {
        max-width: 70%;
        padding: 8px 12px;
        border-radius: 18px;
        background: white;
        box-shadow: 0 1px 2px rgba(0, 0, 0, 0.1);
        font-size: 14px;
        word-wrap: break-word;
      }

      .own-message .message-content {
        background: #0084ff;
        color: white;
      }

      .message-time {
        font-size: 11px;
        color: #65676b;
        margin-top: 2px;
        padding: 0 4px;
      }

      .typing-indicator {
        font-size: 12px;
        color: #65676b;
        font-style: italic;
        padding: 5px;
      }

      .chat-box-input {
        display: flex;
        padding: 10px;
        border-top: 1px solid #e0e0e0;
        background: white;
      }

      .chat-box-input input {
        flex: 1;
        padding: 8px 12px;
        border: 1px solid #e0e0e0;
        border-radius: 20px;
        outline: none;
        font-size: 14px;
      }

      .chat-box-input button {
        margin-left: 8px;
        padding: 8px 16px;
        background: #1877f2;
        color: white;
        border: none;
        border-radius: 20px;
        cursor: pointer;
        font-weight: 600;
      }

      .chat-box-input button:hover {
        background: #166fe5;
      }
    `,
  ],
})
export class ChatBoxComponent implements OnInit, OnDestroy {
  @Input() conversation!: ConversationDto;
  @Input() position!: number; // For positioning multiple chat boxes
  @Output() closeChatBoxEvent = new EventEmitter<number>();
  @ViewChild('messageContainer') private messageContainer!: ElementRef;

  messages: MessageDto[] = [];
  newMessage: string = '';
  otherUserName: string = '';
  isMinimized: boolean = false;
  typingUser: boolean = false;
  private destroy$ = new Subject<void>();
  private currentUserId: string = '';
  private tempMessageRefs = new Map<number, { content: string; senderId: string }>();

  constructor(
    private chatService: ChatSignalRService,
    private el: ElementRef,
  ) {}

  ngOnInit(): void {
    this.currentUserId = this.getCurrentUserId();
    this.otherUserName = this.getOtherUserName();

    // Position the chat box based on index
    const element = this.el.nativeElement.querySelector('.chat-box');
    if (element) {
      element.style.right = `${20 + this.position * 320}px`;
    }

    this.loadMessages();
    this.joinConversation();
    this.setupSignalRListeners();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.chatService.leaveConversation(this.conversation.id);
  }

  private loadMessages(): void {
    this.chatService.getMessages(this.conversation.id).subscribe({
      next: (messages) => {
        this.messages = messages;
        setTimeout(() => this.scrollToBottom(), 100);
      },
      error: (err) => console.error('Error loading messages:', err),
    });
  }

  private joinConversation(): void {
    this.chatService.joinConversation(this.conversation.id);
    this.chatService.markAsRead(this.conversation.id);
  }

  private setupSignalRListeners(): void {
    this.chatService.messageReceived$.pipe(takeUntil(this.destroy$)).subscribe((message) => {
      if (message.conversationId === this.conversation.id) {
        const tempEntry = Array.from(this.tempMessageRefs.entries()).find(
          ([id, ref]) => ref.senderId === message.senderId && ref.content === message.content,
        );

        if (tempEntry) {
          const [tempId] = tempEntry;
          const index = this.messages.findIndex((m) => m.id === tempId);
          if (index !== -1) {
            this.messages[index] = message;
          }
          this.tempMessageRefs.delete(tempId);
        } else {
          const alreadyExists = this.messages.some((m) => m.id === message.id);
          if (!alreadyExists) {
            this.messages = [...this.messages, message];
          }
        }

        setTimeout(() => this.scrollToBottom(), 100);
        if (message.senderId !== this.currentUserId) {
          this.chatService.markAsRead(this.conversation.id);
        }
      }
    });

    this.chatService.userTyping$
      .pipe(takeUntil(this.destroy$))
      .subscribe(({ conversationId, userId }) => {
        if (conversationId === this.conversation.id && userId !== this.currentUserId) {
          this.typingUser = true;
          setTimeout(() => (this.typingUser = false), 2000);
        }
      });
  }

  sendMessage(): void {
    if (!this.newMessage.trim()) return;

    const messageContent = this.newMessage;
    this.newMessage = '';

    const tempId = Date.now();
    const tempMessage: MessageDto = {
      id: tempId,
      conversationId: this.conversation.id,
      senderId: this.currentUserId,
      senderName: this.getSenderName(),
      content: messageContent,
      sentAt: new Date().toISOString(),
      isRead: false,
    };
    this.messages = [...this.messages, tempMessage];
    setTimeout(() => this.scrollToBottom(), 100);

    this.tempMessageRefs.set(tempId, {
      content: messageContent,
      senderId: this.currentUserId,
    });

    this.chatService.sendMessage(this.conversation.id, messageContent);
  }

  onTyping(): void {
    this.chatService.sendTypingIndicator(this.conversation.id);
  }

  toggleMinimize(): void {
    this.isMinimized = !this.isMinimized;
  }

  closeChat(event: Event): void {
    event.stopPropagation();
    this.closeChatBoxEvent.emit(this.conversation.id);
  }

  isOwnMessage(msg: MessageDto): boolean {
    return msg.senderId === this.currentUserId;
  }

  private getOtherUserName(): string {
    return this.currentUserId === this.conversation.customerId
      ? this.conversation.sellerName
      : this.conversation.customerName;
  }

  private getSenderName(): string {
    return this.currentUserId === this.conversation.customerId
      ? this.conversation.customerName
      : this.conversation.sellerName;
  }

  private getCurrentUserId(): string {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      const user = JSON.parse(userStr);
      return user.id || '';
    }
    return '';
  }

  private scrollToBottom(): void {
    if (this.messageContainer) {
      const element = this.messageContainer.nativeElement;
      element.scrollTop = element.scrollHeight;
    }
  }
}

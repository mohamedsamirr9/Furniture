export interface ConversationDto {
  id: number;
  sellerId: string;
  sellerName: string;
  customerId: string;
  customerName: string;
  createdAt: string;
  lastMessage: MessageDto | null;
  unreadCount: number;
}

export interface MessageDto {
  id: number;
  conversationId: number;
  senderId: string;
  senderName: string;
  content: string;
  sentAt: string;
  isRead: boolean;
}

export interface StartConversationDto {
  otherUserId: string;
  firstMessage: string;
}

export interface SendMessageDto {
  conversationId: number;
  content: string;
}

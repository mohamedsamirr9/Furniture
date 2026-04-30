export interface Notification {   
  id: number;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
  customRequestId?: number;
}

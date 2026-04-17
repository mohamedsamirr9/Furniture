export interface Complaint {
  id: number;
  userId: string;
  userName?: string;
  orderId?: number;
  orderNumber?: string;
  imageUrl?: string;
  sellerId?: string;
  productId?: number;
  description: string;
  status: ComplaintStatus;
  createdAt: string;
  updatedAt?: string;
  latestReplyMessage?: string;
  latestReplyBy?: string;
  latestReplyAt?: string;
  replies?: ComplaintReply[];
}

export enum ComplaintStatus {
  Open = 'Open',
  InProgress = 'InProgress',
  Resolved = 'Resolved',
  Closed = 'Closed',
}

export interface CreateComplaint {
  orderId: number;
  description: string;
  imageUrl?: string;
}

export interface UpdateComplaint {
  description?: string;
  imageUrl?: string;
}

export interface UpdateComplaintStatus {
  status: ComplaintStatus;
}

export interface ReplyComplaint {
  message: string;
}

export interface ComplaintReply {
  id: number;
  message: string;
  responderId: string;
  responderName: string;
  createdAt: string;
}

export interface ComplaintDetail extends Complaint {
  userId: string;
  userName: string;
  sellerName?: string;
  replies: ComplaintReply[];
}
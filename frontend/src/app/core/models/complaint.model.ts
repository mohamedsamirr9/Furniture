export interface Complaint {
  id: number;
  userId: string;
  userName?: string;
  orderId?: number;
  orderNumber?: string;
  description: string;
  status: ComplaintStatus;
  createdAt: string;
  updatedAt?: string;
}

export enum ComplaintStatus {
  Open = 'Open',
  InProgress = 'InProgress',
  Resolved = 'Resolved',
  Closed = 'Closed'
}

export interface CreateComplaint {
  orderId: number;
  description: string;
}

export interface UpdateComplaint {
  status?: ComplaintStatus;
  description?: string;
}
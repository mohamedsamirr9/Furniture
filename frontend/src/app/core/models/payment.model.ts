export interface CreatePaymentRequest {
  orderId: number;
  paymentMethod?: 'cash' | 'card';
}

export interface PaymentResponse {
  paymentUrl: string;
  paymentToken: string;
  orderId: number;
  amount: number;
  message: string;
}

export interface VerifyPaymentResponse {
  isPaid: boolean;
}

// Seller payment dashboard interfaces
export interface SellerEarnings {
  totalSales: number;
  totalCommission: number;
  netEarnings: number;
  pendingAmount: number;
  paidAmount: number;
}

export interface SellerPayout {
  id: number;
  orderId: number;
  amount: number;
  commissionAmount: number;
  status: string;
  createdAt: string;
  paidAt?: string;
  transactionId?: string;
}

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

export interface RecentPayout {
  orderId: number;
  amount: number;
  status: 'Pending' | 'Paid' | string;
  date: string;
}

export interface SellerPaymentDashboard {
  onlineEarnings: {
    totalEarnings: number;
    pendingPayout: number;
    totalPaid: number;
  };
  cashSummary: {
    totalCashOrders: number;
    cashAmount: number;
    pendingCommission: number;
    maxLimit: number;
    remainingLimit: number;
  };
  recentPayouts: RecentPayout[];
}

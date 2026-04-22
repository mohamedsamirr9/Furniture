export interface CreatePaymentRequest {
  orderId: number;
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


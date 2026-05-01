export interface OrderItem {
  productId: number;
  productName: string;
  productImage: string;
  unitPrice: number;
  quantity: number;
  total?: number;
  sellerIsBlocked?: boolean;
  isBlocked?: boolean;
}

export interface Order {
  id: number;
  sellerId?: string;
  subTotal: number;
  shippingCost: number;
  totalPrice: number;
  orderDate: string;
  status: string;
  paymentStatus?: 'Unpaid' | 'Paid' | 'Failed' | 0 | 1 | 2;
  paymentMethod?: 'Cash' | 'Card' | string | null;
  shippingAddress: string;
  createdAt: string;
  userName?: string;
  description?: string;
  imageUrl?: string;
  isCustom?: boolean;
  orderItems: OrderItem[];
}

export interface CreateOrder {
  city: string;
  shippingAddress: string;
  notes?: string;
}
export interface CreateOrderFromOffer {
  offerId: number;
  city: string;
  shippingAddress: string;
  notes?: string;
}

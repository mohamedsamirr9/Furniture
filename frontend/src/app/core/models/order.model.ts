export interface OrderItem {
  productId: number;
  productName: string;
  productImage: string;
  unitPrice: number;
  quantity: number;
  total: number;
}

export interface Order {
  id: number;
  subTotal: number;
  shippingCost: number;
  totalPrice: number;
  orderDate: string;
  status: string;
  shippingAddress: string;
  createdAt: string;
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

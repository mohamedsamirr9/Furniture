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
  totalPrice: number;
  orderDate: string;
  status: string;
  shippingAddress: string;
  createdAt: string;
  orderItems: OrderItem[];
}

export interface CreateOrder {
  shippingAddress: string;
  notes?: string;
}

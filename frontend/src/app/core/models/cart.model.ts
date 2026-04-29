export interface CartItem {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  productImage?: string;
  availableStock: number;
  sellerIsBlocked?: boolean;
  isBlocked?: boolean;
  totalPrice?: number;
}

export interface Cart {
  id?: number | string;
  userId?: string;
  items: CartItem[];
  totalPrice: number;
}

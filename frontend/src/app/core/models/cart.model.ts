export interface CartItem {
  productId: number;
  productName: string;
  price: number;
  quantity: number;
  imageUrl?: string;
  totalPrice?: number;
}

export interface Cart {
  id?: number | string;
  userId?: string;
  items: CartItem[];
  totalAmount: number;
}

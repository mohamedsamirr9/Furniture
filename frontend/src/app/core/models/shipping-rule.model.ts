export interface ShippingRule {
  id: number;
  city: string;
  categoryId: number;
  categoryName: string;
  price: number;
}

export interface ShippingRuleCreateUpdate {
  city: string;
  categoryId: number;
  price: number;
}

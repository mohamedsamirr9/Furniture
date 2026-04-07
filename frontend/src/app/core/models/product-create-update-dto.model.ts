export interface ProductCreateUpdateDto {
  name: string;
  description?: string;
  price: number;
  stockQuantity: number;
  categoryId: number;
  sellerId: string;
  imageUrls?: string[];
}

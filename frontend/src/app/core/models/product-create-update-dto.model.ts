export interface ProductCreateUpdateDto {
  nameEn: string;
  nameAr?: string;
  descriptionEn: string;
  descriptionAr?: string;
  price: number;
  stockQuantity: number;
  categoryId: number;
  sellerId: string;
  imageUrls?: string[];
}

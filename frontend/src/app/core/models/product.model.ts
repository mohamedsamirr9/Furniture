export interface Product {
    id: number;
    nameEn: string;
    nameAr?: string;
    descriptionEn: string;
    descriptionAr?: string;
    name: string;
    description?: string;
    price: number;
    stockQuantity: number;
    categoryId: number;
    categoryName?: string;
    sellerId?: string;
    sellerName?: string;
    sellerIsBlocked?: boolean;
    isBlocked?: boolean;
    averageRating: number;
    mainImage?: string;
    images?: string[];
}

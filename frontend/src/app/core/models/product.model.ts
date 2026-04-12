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
    sellerName?: string;
    mainImage?: string;
    images?: string[];
}

export interface QuizDto {
  style: string;
  color: string;
  roomSize: string;
  budget: string;
}

export interface ProductRecommendationDto {
  id: number;
  name: string;
  price: number;
  imageUrl: string | null;
}

export interface ActionDto {
  productId: number;
  actionType: 'cart' | 'favorite' | 'click';
}

export interface QuizStatusDto {
  isCompleted: boolean;
}
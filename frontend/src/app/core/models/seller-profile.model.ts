/** Raw API response (camelCase from ASP.NET). */
export interface SellerProfileApiDto {
  id: string;
  sellerId?: string;
  name: string;
  email?: string | null;
  location: string;
  joinDate: string;
  rating: number;
  reviewsCount: number;
  completedOrders: number;
  bio: string;
  avatarUrl?: string;
  profileImageUrl?: string;
  specialties: string[];
  portfolio: SellerPortfolioItem[];
  bankName?: string;
  bankAccountNumber?: string;
  bankCode?: string;
  nationalId?: string;
  paymobMerchantId?: string;
  pendingCommission?: number;
  maxAllowedCommission?: number;
  isBlocked?: boolean;
}

export interface SellerPortfolioItem {
  id: number;
  category: string;
  title: string;
  description: string;
  imageUrl: string;
}

/** Normalized view model used by public and dashboard UIs. */
export interface SellerProfileViewModel {
  sellerId: string;
  name: string;
  email?: string;
  location: string;
  joinDate: string;
  rating: number;
  reviewsCount: number;
  completedOrders: number;
  bio: string;
  profileImageUrl: string;
  specialties: string[];
  portfolio: SellerPortfolioItem[];
  bankName?: string;
  bankAccountNumber?: string;
  bankCode?: string;
  nationalId?: string;
  paymobMerchantId?: string;
  pendingCommission?: number;
  maxAllowedCommission?: number;
  isBlocked?: boolean;
}

export function mapSellerProfileApiToView(dto: SellerProfileApiDto): SellerProfileViewModel {
  const img = (dto.profileImageUrl || dto.avatarUrl || '').trim();
  return {
    sellerId: dto.sellerId || dto.id,
    name: dto.name,
    email: dto.email ?? undefined,
    location: dto.location,
    joinDate: dto.joinDate,
    rating: dto.rating,
    reviewsCount: dto.reviewsCount,
    completedOrders: dto.completedOrders,
    bio: dto.bio,
    profileImageUrl: img,
    specialties: dto.specialties ?? [],
    portfolio: dto.portfolio ?? [],
    bankName: dto.bankName,
    bankAccountNumber: dto.bankAccountNumber,
    bankCode: dto.bankCode,
    nationalId: dto.nationalId,
    paymobMerchantId: dto.paymobMerchantId,
    pendingCommission: dto.pendingCommission,
    maxAllowedCommission: dto.maxAllowedCommission,
    isBlocked: dto.isBlocked,
  };
}

export interface UpdateSellerProfilePayload {
  name?: string;
  location?: string;
  bio?: string;
  profileImageUrl?: string;
  // Bank/payment details
  bankName?: string;
  bankAccountNumber?: string;
  bankCode?: string;
  nationalId?: string;
  paymobMerchantId?: string;
}

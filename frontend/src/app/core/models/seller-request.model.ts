export type SellerRequestStatus = 'Pending' | 'Approved' | 'Rejected' | string;

export interface SellerRequestDto {
  id: number;
  userId: string;
  userEmail?: string | null;
  userName?: string | null;
  storeName: string;
  nationalIdImageUrl?: string | null;
  status: SellerRequestStatus;
  createdAt: string;
  reviewedAt?: string | null;
  reviewedById?: string | null;
  rejectionReason?: string | null;
}

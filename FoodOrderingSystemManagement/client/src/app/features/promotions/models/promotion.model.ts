export type PromotionType =
  | 'Coupon' | 'PromoCode' | 'HappyHour' | 'ComboOffer'
  | 'BuyOneGetOne' | 'FestivalOffer' | 'LoyaltyReward' | 'GiftCard';

export type DiscountType = 'Percentage' | 'FlatAmount' | 'FreeItem' | 'PointsMultiplier';

export type PromotionStatus = 'Active' | 'Inactive' | 'Expired' | 'Upcoming' | 'Exhausted';

export interface PromotionDto {
  id: number;
  name: string;
  description?: string;
  promotionType: PromotionType;
  discountType: DiscountType;
  discountValue: number;
  code?: string;
  minOrderValue?: number;
  maxDiscount?: number;
  usageLimit?: number;
  usedCount: number;
  usageLimitPerCustomer?: number;
  startDate?: string;
  endDate?: string;
  happyHourStart?: string;
  happyHourEnd?: string;
  happyHourDays?: string;
  applicableItemIds?: string;
  buyQty?: number;
  getQty?: number;
  giftCardBalance?: number;
  giftCardUsed?: number;
  spendThreshold?: number;
  pointsMultiplierVal?: number;
  isActive: boolean;
  isPublic: boolean;
  bannerColor?: string;
  badgeIcon?: string;
  status: PromotionStatus;
  createdDate: string;
}

export type CreatePromotionDto = Omit<PromotionDto, 'id' | 'usedCount' | 'status' | 'createdDate' | 'giftCardUsed'>;

export interface ApplyPromoCodeDto { code: string; orderTotal: number; }

export interface ApplyPromotionResultDto {
  isValid: boolean;
  message: string;
  discountAmount: number;
  discountType?: string;
  discountValue: number;
  promotionId?: number;
  promotionName?: string;
}

export const PROMO_TYPE_META: Record<PromotionType, { label: string; icon: string; color: string; defaultBadge: string }> = {
  Coupon:        { label: 'Coupon',          icon: 'local_offer',        color: '#e53935', defaultBadge: '🎫' },
  PromoCode:     { label: 'Promo Code',      icon: 'confirmation_number', color: '#7b1fa2', defaultBadge: '🏷️' },
  HappyHour:     { label: 'Happy Hour',      icon: 'schedule',           color: '#f57c00', defaultBadge: '⏰' },
  ComboOffer:    { label: 'Combo Offer',     icon: 'local_dining',       color: '#00695c', defaultBadge: '🍱' },
  BuyOneGetOne:  { label: 'Buy 1 Get 1',     icon: 'redeem',             color: '#1565c0', defaultBadge: '🎁' },
  FestivalOffer: { label: 'Festival Offer',  icon: 'celebration',        color: '#ad1457', defaultBadge: '🎉' },
  LoyaltyReward: { label: 'Loyalty Reward',  icon: 'stars',              color: '#ff6f00', defaultBadge: '⭐' },
  GiftCard:      { label: 'Gift Card',       icon: 'card_giftcard',      color: '#2e7d32', defaultBadge: '💳' },
};

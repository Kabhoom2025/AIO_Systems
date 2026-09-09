export interface Customer {
  id: number;
  name: string;
  phone: string;
  email?: string;
  birthday?: string;
  notes?: string;
  isActive: boolean;
  loyaltyPoints: number;
  totalSpend: number;
  totalVisits: number;
  tier: 'Bronze' | 'Silver' | 'Gold';
  createdDate: string;
}

export interface CreateCustomerRequest {
  name: string;
  phone: string;
  email?: string;
  birthday?: string;
  notes?: string;
}

export interface UpdateCustomerRequest {
  name: string;
  email?: string;
  birthday?: string;
  notes?: string;
  isActive: boolean;
}

export interface AdjustPointsRequest {
  points: number;
  description: string;
}

export interface PointsTransaction {
  id: number;
  points: number;
  type: 'Earn' | 'Redeem' | 'Adjust';
  description: string;
  orderId?: number;
  createdDate: string;
}

export interface SavedAddress {
  id: number;
  label: string;
  addressLine: string;
  city?: string;
  isDefault: boolean;
}

export interface AddSavedAddressRequest {
  label: string;
  addressLine: string;
  city?: string;
  isDefault: boolean;
}

export interface CustomerOrderSummary {
  id: number;
  orderNumber: string;
  orderDate: string;
  grandTotal: number;
  status: string;
  pointsEarned: number;
  pointsRedeemed: number;
}

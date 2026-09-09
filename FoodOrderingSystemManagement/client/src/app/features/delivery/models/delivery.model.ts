export interface DriverDto {
  id: number;
  name: string;
  phone: string;
  email?: string;
  vehicleNo?: string;
  vehicleType?: string;
  isAvailable: boolean;
  isActive: boolean;
  activeDeliveries: number;
  currentLatitude?: number | null;
  currentLongitude?: number | null;
  lastLocationUpdate?: string | null;
}

export interface CreateDriverDto {
  name: string;
  phone: string;
  email?: string;
  vehicleNo?: string;
  vehicleType?: string;
}

export interface UpdateDriverDto {
  name: string;
  phone: string;
  email?: string;
  vehicleNo?: string;
  vehicleType?: string;
  isAvailable: boolean;
  isActive: boolean;
}

export interface DeliveryOrderDto {
  id: number;
  orderId: number;
  orderNumber: string;
  orderTotal: number;
  driverId?: number;
  driverName?: string;
  driverPhone?: string;
  status: string;
  deliveryAddress: string;
  deliveryLatitude?: number | null;
  deliveryLongitude?: number | null;
  deliveryCharge: number;
  customerPhone?: string;
  notes?: string;
  assignedAt?: string;
  pickedUpAt?: string;
  deliveredAt?: string;
  thirdPartyProvider?: string;
  thirdPartyTrackId?: string;
  createdDate: string;
}

export interface CreateDeliveryOrderDto {
  orderId: number;
  deliveryAddress: string;
  deliveryLatitude?: number | null;
  deliveryLongitude?: number | null;
  deliveryCharge: number;
  customerPhone?: string;
  notes?: string;
}

export interface AssignDriverDto {
  driverId: number;
}

export interface UpdateDeliveryStatusDto {
  status: string;
  notes?: string;
}

export interface DeliveryChargeSlabDto {
  id: number;
  fromKm: number;
  toKm: number;
  charge: number;
}

export interface UpsertDeliveryChargeSlabDto {
  fromKm: number;
  toKm: number;
  charge: number;
}

export interface ThirdPartyConfigDto {
  id: number;
  provider: string;
  apiKey: string;
  apiSecret?: string;
  webhookUrl?: string;
  isEnabled: boolean;
}

export interface UpsertThirdPartyConfigDto {
  provider: string;
  apiKey: string;
  apiSecret?: string;
  webhookUrl?: string;
  isEnabled: boolean;
}

export interface DeliveryDashboardDto {
  totalToday: number;
  pendingCount: number;
  activeCount: number;
  deliveredToday: number;
  failedToday: number;
  availableDrivers: number;
  totalChargeToday: number;
}

export const DELIVERY_STATUSES = [
  'Pending', 'Assigned', 'PickedUp', 'OutForDelivery', 'Delivered', 'Failed', 'Cancelled'
] as const;

export type DeliveryStatus = typeof DELIVERY_STATUSES[number];

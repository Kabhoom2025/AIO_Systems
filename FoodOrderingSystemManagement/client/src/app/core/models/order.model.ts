export interface OrderItemRequest {
  foodItemId: number;
  quantity: number;
  addOnIds?: number[];
}

export interface CreateOrderRequest {
  tableId?: number | null;
  customerPhone?: string | null;
  customerId?: number;
  pointsRedeemed?: number;
  discount?: number;
  orderType?: 'DineIn' | 'Takeaway' | 'Delivery';
  deliveryAddress?: string;
  deliveryCharge?: number;
  items: OrderItemRequest[];
}

export interface AddItemsRequest {
  items: OrderItemRequest[];
  discount?: number;
}

export interface OrderItem {
  foodItemId: number;
  itemName: string;
  image: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
  addOnNotes?: string;
  isKitchenReady?: boolean;
  isDelivered?: boolean;
}

export interface Order {
  id: number;
  orderNumber: string;
  cashierName: string;
  orderDate: string;
  createdDate: string;
  subTotal: number;
  tax: number;
  discount: number;
  grandTotal: number;
  status: string;
  tableId: number | null;
  tableNumber: number | null;
  customerPhone: string | null;
  orderType: 'DineIn' | 'Takeaway' | 'Delivery';
  deliveryAddress?: string | null;
  deliveryCharge?: number;
  customerName?: string | null;
  customerTier?: string | null;
  pointsEarned?: number;
  pointsRedeemed?: number;
  newPointsBalance?: number;
  items: OrderItem[];
}

// Local cart item (not sent to API — used only in the order panel)
export interface CartItem {
  foodItemId: number;
  itemName: string;
  image: string;
  price: number;
  quantity: number;
  selectedAddOnIds: number[];
  addOnNames: string;
  addOnTotal: number;
}

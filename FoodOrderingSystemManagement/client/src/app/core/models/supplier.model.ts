export interface Supplier {
  id: number;
  name: string;
  contactPerson?: string;
  email?: string;
  phone?: string;
  address?: string;
  taxNumber?: string;
  paymentTerms?: string;
  isActive: boolean;
  notes?: string;
  purchaseOrderCount: number;
  totalPaid: number;
}

export interface CreateSupplierRequest {
  name: string;
  contactPerson?: string;
  email?: string;
  phone?: string;
  address?: string;
  taxNumber?: string;
  paymentTerms?: string;
  notes?: string;
}

export interface UpdateSupplierRequest extends CreateSupplierRequest {
  isActive: boolean;
}

export interface PurchaseOrderItem {
  id: number;
  inventoryItemId: number;
  itemName: string;
  unit?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  receivedQuantity?: number;
}

export interface PurchaseOrder {
  id: number;
  poNumber: string;
  supplierId: number;
  supplierName: string;
  status: string;
  orderDate: string;
  expectedDate?: string;
  receivedDate?: string;
  totalAmount: number;
  notes?: string;
  items: PurchaseOrderItem[];
}

export interface CreatePurchaseOrderItemRequest {
  inventoryItemId: number;
  itemName: string;
  unit?: string;
  quantity: number;
  unitPrice: number;
}

export interface CreatePurchaseOrderRequest {
  supplierId: number;
  expectedDate?: string;
  notes?: string;
  items: CreatePurchaseOrderItemRequest[];
}

export interface UpdatePurchaseOrderStatusRequest {
  status: string;
  receivedDate?: string;
  notes?: string;
}

export interface SupplierPayment {
  id: number;
  supplierId: number;
  supplierName: string;
  purchaseOrderId?: number;
  poNumber?: string;
  amount: number;
  paymentMethod: string;
  referenceNumber?: string;
  paymentDate: string;
  notes?: string;
}

export interface CreateSupplierPaymentRequest {
  supplierId: number;
  purchaseOrderId?: number;
  amount: number;
  paymentMethod: string;
  referenceNumber?: string;
  paymentDate: string;
  notes?: string;
}

export const PO_STATUSES = ['Draft','Sent','Confirmed','PartiallyReceived','Received','Cancelled'];
export const PAYMENT_METHODS = ['Cash','Bank Transfer','Cheque','UPI','Card'];

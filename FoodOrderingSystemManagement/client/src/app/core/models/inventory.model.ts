export interface InventoryItem {
  id: number;
  name: string;
  description?: string;
  unit: string;
  currentStock: number;
  minimumStock: number;
  isActive: boolean;
  isLowStock: boolean;
  barcode?: string;
  lastUpdated: string;
  expiryDate?: string;
}

export interface CreateInventoryItemRequest {
  name: string;
  description?: string;
  unit: string;
  currentStock: number;
  minimumStock: number;
  barcode?: string;
  expiryDate?: string;
}

export interface UpdateInventoryItemRequest extends CreateInventoryItemRequest {
  isActive: boolean;
}

export interface StockAdjustmentRequest {
  quantity: number;
  note?: string;
}

export interface PurchaseStockRequest {
  quantity: number;
  supplier?: string;
  referenceNumber?: string;
  batchNumber?: string;
  expiryDate?: string;
  notes?: string;
}

export interface WasteStockRequest {
  quantity: number;
  wasteReason?: string;
  batchNumber?: string;
  notes?: string;
}

export interface TransferStockRequest {
  targetInventoryItemId: number;
  quantity: number;
  notes?: string;
}

export interface StockTransaction {
  id: number;
  transactionType: string;
  quantity: number;
  stockBefore: number;
  stockAfter: number;
  batchNumber?: string;
  expiryDate?: string;
  supplier?: string;
  referenceNumber?: string;
  notes?: string;
  relatedTransactionId?: number;
  createdAt: string;
}

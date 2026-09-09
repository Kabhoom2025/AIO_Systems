import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import {
  InventoryItem,
  CreateInventoryItemRequest,
  UpdateInventoryItemRequest,
  StockAdjustmentRequest,
  PurchaseStockRequest,
  WasteStockRequest,
  TransferStockRequest,
  StockTransaction,
} from '../models/inventory.model';

@Injectable({ providedIn: 'root' })
export class InventoryService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/inventory`;

  getAll(): Observable<ApiResponse<InventoryItem[]>> {
    return this.http.get<ApiResponse<InventoryItem[]>>(this.apiUrl);
  }

  getById(id: number): Observable<ApiResponse<InventoryItem>> {
    return this.http.get<ApiResponse<InventoryItem>>(`${this.apiUrl}/${id}`);
  }

  getLowStock(): Observable<ApiResponse<InventoryItem[]>> {
    return this.http.get<ApiResponse<InventoryItem[]>>(`${this.apiUrl}/low-stock`);
  }

  getExpiring(days = 7): Observable<ApiResponse<InventoryItem[]>> {
    return this.http.get<ApiResponse<InventoryItem[]>>(`${this.apiUrl}/expiring?days=${days}`);
  }

  getTransactions(id: number): Observable<ApiResponse<StockTransaction[]>> {
    return this.http.get<ApiResponse<StockTransaction[]>>(`${this.apiUrl}/${id}/transactions`);
  }

  create(payload: CreateInventoryItemRequest): Observable<ApiResponse<InventoryItem>> {
    return this.http.post<ApiResponse<InventoryItem>>(this.apiUrl, payload);
  }

  update(id: number, payload: UpdateInventoryItemRequest): Observable<ApiResponse<InventoryItem>> {
    return this.http.put<ApiResponse<InventoryItem>>(`${this.apiUrl}/${id}`, payload);
  }

  adjustStock(id: number, payload: StockAdjustmentRequest): Observable<ApiResponse<InventoryItem>> {
    return this.http.patch<ApiResponse<InventoryItem>>(`${this.apiUrl}/${id}/adjust`, payload);
  }

  purchaseStock(id: number, payload: PurchaseStockRequest): Observable<ApiResponse<InventoryItem>> {
    return this.http.post<ApiResponse<InventoryItem>>(`${this.apiUrl}/${id}/purchase`, payload);
  }

  wasteStock(id: number, payload: WasteStockRequest): Observable<ApiResponse<InventoryItem>> {
    return this.http.post<ApiResponse<InventoryItem>>(`${this.apiUrl}/${id}/waste`, payload);
  }

  transferStock(id: number, payload: TransferStockRequest): Observable<ApiResponse<{ source: InventoryItem; target: InventoryItem }>> {
    return this.http.post<ApiResponse<{ source: InventoryItem; target: InventoryItem }>>(`${this.apiUrl}/${id}/transfer`, payload);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.apiUrl}/${id}`);
  }

  getByBarcode(code: string): Observable<ApiResponse<InventoryItem>> {
    return this.http.get<ApiResponse<InventoryItem>>(`${this.apiUrl}/barcode/${encodeURIComponent(code)}`);
  }
}

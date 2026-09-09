import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { AddItemsRequest, CreateOrderRequest, Order } from '../models/order.model';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly apiUrl = `${environment.apiUrl}/order`;
  private http   = inject(HttpClient);
  private dialog = inject(MatDialog);

  getAll(): Observable<ApiResponse<Order[]>> {
    return this.http.get<ApiResponse<Order[]>>(this.apiUrl);
  }

  getPending(): Observable<ApiResponse<Order[]>> {
    return this.http.get<ApiResponse<Order[]>>(`${this.apiUrl}/pending`);
  }

  getById(id: number): Observable<ApiResponse<Order>> {
    return this.http.get<ApiResponse<Order>>(`${this.apiUrl}/${id}`);
  }

  create(payload: CreateOrderRequest): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(this.apiUrl, payload);
  }

  addItems(orderId: number, payload: AddItemsRequest): Observable<ApiResponse<Order>> {
    return this.http.post<ApiResponse<Order>>(`${this.apiUrl}/${orderId}/add-items`, payload);
  }

  generateBill(orderId: number, discount?: number | null): Observable<ApiResponse<any>> {
    const body = discount != null ? { discount } : {};
    return this.http.patch<ApiResponse<any>>(`${this.apiUrl}/${orderId}/generate-bill`, body);
  }

  cancel(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.apiUrl}/${id}/cancel`, {});
  }

  transferTable(orderId: number, newTableId: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.apiUrl}/${orderId}/transfer-table`, { newTableId });
  }

  transferToTakeaway(orderId: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.apiUrl}/${orderId}/transfer-takeaway`, {});
  }

  markPickedUp(orderId: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.apiUrl}/${orderId}/mark-served`, {});
  }

  confirmPayment(orderId: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.apiUrl}/${orderId}/confirm-payment`, {});
  }

  reopen(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.apiUrl}/${id}/reopen`, {});
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.apiUrl}/${id}`);
  }

  openReceipt(order: any): void {
    import('../../features/orders/receipt/receipt-dialog.component').then((m) => {
      this.dialog.open(m.ReceiptDialogComponent, {
        data: order,
        width: '480px',
        disableClose: false,
      });
    });
  }
}

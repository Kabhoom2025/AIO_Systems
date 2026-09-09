import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Supplier, CreateSupplierRequest, UpdateSupplierRequest,
  PurchaseOrder, CreatePurchaseOrderRequest, UpdatePurchaseOrderStatusRequest,
  SupplierPayment, CreateSupplierPaymentRequest,
} from '../models/supplier.model';

@Injectable({ providedIn: 'root' })
export class SupplierService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/suppliers`;

  getAll(): Observable<Supplier[]> { return this.http.get<Supplier[]>(this.base); }
  getById(id: number): Observable<Supplier> { return this.http.get<Supplier>(`${this.base}/${id}`); }
  create(req: CreateSupplierRequest): Observable<Supplier> { return this.http.post<Supplier>(this.base, req); }
  update(id: number, req: UpdateSupplierRequest): Observable<Supplier> { return this.http.put<Supplier>(`${this.base}/${id}`, req); }
  delete(id: number): Observable<void> { return this.http.delete<void>(`${this.base}/${id}`); }

  getPurchaseOrders(supplierId: number): Observable<PurchaseOrder[]> {
    return this.http.get<PurchaseOrder[]>(`${this.base}/${supplierId}/purchase-orders`);
  }
  createPurchaseOrder(req: CreatePurchaseOrderRequest): Observable<PurchaseOrder> {
    return this.http.post<PurchaseOrder>(`${this.base}/purchase-orders`, req);
  }
  updatePurchaseOrderStatus(orderId: number, req: UpdatePurchaseOrderStatusRequest): Observable<PurchaseOrder> {
    return this.http.patch<PurchaseOrder>(`${this.base}/purchase-orders/${orderId}/status`, req);
  }

  getPayments(supplierId: number): Observable<SupplierPayment[]> {
    return this.http.get<SupplierPayment[]>(`${this.base}/${supplierId}/payments`);
  }
  createPayment(req: CreateSupplierPaymentRequest): Observable<SupplierPayment> {
    return this.http.post<SupplierPayment>(`${this.base}/payments`, req);
  }
}

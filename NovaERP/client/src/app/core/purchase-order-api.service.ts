import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PurchaseOrderLineDto {
  id: number;
  itemName: string;
  quantity: number;
  unitPrice: number;
  taxCodeId: number | null;
  taxCodeName: string | null;
  taxRatePercent: number;
  productId: number | null;
  productName: string | null;
  displayOrder: number;
  lineSubtotal: number;
  lineTax: number;
  lineTotal: number;
}

export interface CreatePurchaseOrderLineDto {
  itemName: string;
  quantity: number;
  unitPrice: number;
  taxCodeId: number | null;
  productId: number | null;
  displayOrder: number;
}

export interface PurchaseOrderDto {
  id: number;
  poNumber: string;
  vendorId: number;
  vendorName: string;
  rfqRequestId: number | null;
  rfqNumber: string | null;
  status: string;
  orderDate: string;
  ownerId: number;
  ownerName: string;
  lines: PurchaseOrderLineDto[];
  subtotal: number;
  taxTotal: number;
  grandTotal: number;
}

export interface CreatePurchaseOrderDto {
  vendorId: number;
  rfqRequestId: number | null;
  orderDate: string;
  ownerId: number;
  lines: CreatePurchaseOrderLineDto[];
}

export interface UpdatePurchaseOrderDto {
  rfqRequestId: number | null;
  orderDate: string;
  ownerId: number;
  lines: CreatePurchaseOrderLineDto[];
}

@Injectable({ providedIn: 'root' })
export class PurchaseOrderApiService {
  private ordersUrl = `${environment.apiUrl}/purchase-orders`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<PurchaseOrderDto[]> {
    return this.http.get<PurchaseOrderDto[]>(this.ordersUrl);
  }

  getById(id: number): Observable<PurchaseOrderDto> {
    return this.http.get<PurchaseOrderDto>(`${this.ordersUrl}/${id}`);
  }

  create(dto: CreatePurchaseOrderDto): Observable<PurchaseOrderDto> {
    return this.http.post<PurchaseOrderDto>(this.ordersUrl, dto);
  }

  update(id: number, dto: UpdatePurchaseOrderDto): Observable<PurchaseOrderDto> {
    return this.http.put<PurchaseOrderDto>(`${this.ordersUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.ordersUrl}/${id}`);
  }

  confirm(id: number): Observable<PurchaseOrderDto> {
    return this.http.post<PurchaseOrderDto>(`${this.ordersUrl}/${id}/confirm`, {});
  }

  receive(id: number): Observable<PurchaseOrderDto> {
    return this.http.post<PurchaseOrderDto>(`${this.ordersUrl}/${id}/receive`, {});
  }

  cancel(id: number): Observable<PurchaseOrderDto> {
    return this.http.post<PurchaseOrderDto>(`${this.ordersUrl}/${id}/cancel`, {});
  }
}

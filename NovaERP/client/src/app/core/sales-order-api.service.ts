import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface SalesOrderLineDto {
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

export interface CreateSalesOrderLineDto {
  itemName: string;
  quantity: number;
  unitPrice: number;
  taxCodeId: number | null;
  productId: number | null;
  displayOrder: number;
}

export interface SalesOrderDto {
  id: number;
  orderNumber: string;
  accountId: number;
  accountName: string;
  opportunityId: number | null;
  opportunityName: string | null;
  status: string;
  orderDate: string;
  ownerId: number;
  ownerName: string;
  lines: SalesOrderLineDto[];
  subtotal: number;
  taxTotal: number;
  grandTotal: number;
}

export interface CreateSalesOrderDto {
  accountId: number;
  opportunityId: number | null;
  orderDate: string;
  ownerId: number;
  lines: CreateSalesOrderLineDto[];
}

export interface UpdateSalesOrderDto {
  opportunityId: number | null;
  orderDate: string;
  ownerId: number;
  lines: CreateSalesOrderLineDto[];
}

@Injectable({ providedIn: 'root' })
export class SalesOrderApiService {
  private ordersUrl = `${environment.apiUrl}/sales-orders`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<SalesOrderDto[]> {
    return this.http.get<SalesOrderDto[]>(this.ordersUrl);
  }

  getById(id: number): Observable<SalesOrderDto> {
    return this.http.get<SalesOrderDto>(`${this.ordersUrl}/${id}`);
  }

  create(dto: CreateSalesOrderDto): Observable<SalesOrderDto> {
    return this.http.post<SalesOrderDto>(this.ordersUrl, dto);
  }

  update(id: number, dto: UpdateSalesOrderDto): Observable<SalesOrderDto> {
    return this.http.put<SalesOrderDto>(`${this.ordersUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.ordersUrl}/${id}`);
  }

  confirm(id: number): Observable<SalesOrderDto> {
    return this.http.post<SalesOrderDto>(`${this.ordersUrl}/${id}/confirm`, {});
  }

  cancel(id: number): Observable<SalesOrderDto> {
    return this.http.post<SalesOrderDto>(`${this.ordersUrl}/${id}/cancel`, {});
  }
}

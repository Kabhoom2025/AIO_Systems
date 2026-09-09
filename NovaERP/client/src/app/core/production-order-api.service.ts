import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ProductionOrderDto {
  id: number;
  moNumber: string;
  productId: number;
  productName: string;
  warehouseId: number;
  warehouseName: string;
  quantity: number;
  status: string;
  orderDate: string;
  ownerId: number;
  ownerName: string;
}

export interface CreateProductionOrderDto {
  productId: number;
  warehouseId: number;
  quantity: number;
  orderDate: string;
  ownerId: number;
}

export interface UpdateProductionOrderDto {
  warehouseId: number;
  quantity: number;
  orderDate: string;
  ownerId: number;
}

@Injectable({ providedIn: 'root' })
export class ProductionOrderApiService {
  private ordersUrl = `${environment.apiUrl}/production-orders`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ProductionOrderDto[]> {
    return this.http.get<ProductionOrderDto[]>(this.ordersUrl);
  }

  getById(id: number): Observable<ProductionOrderDto> {
    return this.http.get<ProductionOrderDto>(`${this.ordersUrl}/${id}`);
  }

  create(dto: CreateProductionOrderDto): Observable<ProductionOrderDto> {
    return this.http.post<ProductionOrderDto>(this.ordersUrl, dto);
  }

  update(id: number, dto: UpdateProductionOrderDto): Observable<ProductionOrderDto> {
    return this.http.put<ProductionOrderDto>(`${this.ordersUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.ordersUrl}/${id}`);
  }

  complete(id: number): Observable<ProductionOrderDto> {
    return this.http.post<ProductionOrderDto>(`${this.ordersUrl}/${id}/complete`, {});
  }

  cancel(id: number): Observable<ProductionOrderDto> {
    return this.http.post<ProductionOrderDto>(`${this.ordersUrl}/${id}/cancel`, {});
  }
}

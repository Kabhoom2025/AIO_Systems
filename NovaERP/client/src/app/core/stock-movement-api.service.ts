import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface StockMovementDto {
  id: number;
  productId: number;
  productName: string;
  productSku: string;
  warehouseId: number | null;
  warehouseName: string | null;
  movementType: string;
  quantity: number;
  entityType: string | null;
  entityId: number | null;
  movementDate: string;
  notes: string | null;
}

export interface CreateStockMovementDto {
  productId: number;
  warehouseId: number | null;
  movementType: string;
  quantity: number;
  notes: string | null;
}

@Injectable({ providedIn: 'root' })
export class StockMovementApiService {
  private movementsUrl = `${environment.apiUrl}/stock-movements`;

  constructor(private http: HttpClient) {}

  getAll(productId?: number): Observable<StockMovementDto[]> {
    const url = productId ? `${this.movementsUrl}?productId=${productId}` : this.movementsUrl;
    return this.http.get<StockMovementDto[]>(url);
  }

  create(dto: CreateStockMovementDto): Observable<StockMovementDto> {
    return this.http.post<StockMovementDto>(this.movementsUrl, dto);
  }

  getOnHandAtWarehouse(productId: number, warehouseId: number): Observable<number> {
    return this.http.get<number>(`${this.movementsUrl}/on-hand?productId=${productId}&warehouseId=${warehouseId}`);
  }
}

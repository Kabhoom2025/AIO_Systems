import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface StockTransferDto {
  id: number;
  productId: number;
  productName: string;
  productSku: string;
  fromWarehouseId: number;
  fromWarehouseName: string;
  toWarehouseId: number;
  toWarehouseName: string;
  quantity: number;
  transferDate: string;
  notes: string | null;
}

export interface CreateStockTransferDto {
  productId: number;
  fromWarehouseId: number;
  toWarehouseId: number;
  quantity: number;
  notes: string | null;
}

@Injectable({ providedIn: 'root' })
export class StockTransferApiService {
  private transfersUrl = `${environment.apiUrl}/stock-transfers`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<StockTransferDto[]> {
    return this.http.get<StockTransferDto[]>(this.transfersUrl);
  }

  create(dto: CreateStockTransferDto): Observable<StockTransferDto> {
    return this.http.post<StockTransferDto>(this.transfersUrl, dto);
  }
}

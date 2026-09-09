import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PosSaleLineDto {
  id: number;
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface CreatePosSaleLineDto {
  productId: number;
  quantity: number;
  unitPrice: number;
}

export interface PosSaleDto {
  id: number;
  saleNumber: string;
  warehouseId: number;
  warehouseName: string;
  customerAccountId: number | null;
  customerAccountName: string | null;
  saleDate: string;
  revenueLedgerAccountId: number;
  revenueLedgerAccountName: string;
  paymentLedgerAccountId: number | null;
  paymentLedgerAccountName: string | null;
  status: string;
  ownerId: number;
  ownerName: string;
  postedJournalEntryId: number | null;
  lines: PosSaleLineDto[];
  totalAmount: number;
}

export interface CreatePosSaleDto {
  warehouseId: number;
  customerAccountId: number | null;
  saleDate: string;
  revenueLedgerAccountId: number;
  ownerId: number;
  lines: CreatePosSaleLineDto[];
}

export interface UpdatePosSaleDto {
  warehouseId: number;
  customerAccountId: number | null;
  saleDate: string;
  revenueLedgerAccountId: number;
  ownerId: number;
  lines: CreatePosSaleLineDto[];
}

export interface CompletePosSaleDto {
  paymentLedgerAccountId: number;
}

@Injectable({ providedIn: 'root' })
export class PosSaleApiService {
  private salesUrl = `${environment.apiUrl}/pos-sales`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<PosSaleDto[]> {
    return this.http.get<PosSaleDto[]>(this.salesUrl);
  }

  getById(id: number): Observable<PosSaleDto> {
    return this.http.get<PosSaleDto>(`${this.salesUrl}/${id}`);
  }

  create(dto: CreatePosSaleDto): Observable<PosSaleDto> {
    return this.http.post<PosSaleDto>(this.salesUrl, dto);
  }

  update(id: number, dto: UpdatePosSaleDto): Observable<PosSaleDto> {
    return this.http.put<PosSaleDto>(`${this.salesUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.salesUrl}/${id}`);
  }

  complete(id: number, dto: CompletePosSaleDto): Observable<PosSaleDto> {
    return this.http.post<PosSaleDto>(`${this.salesUrl}/${id}/complete`, dto);
  }

  refund(id: number): Observable<PosSaleDto> {
    return this.http.post<PosSaleDto>(`${this.salesUrl}/${id}/refund`, {});
  }

  cancel(id: number): Observable<PosSaleDto> {
    return this.http.post<PosSaleDto>(`${this.salesUrl}/${id}/cancel`, {});
  }
}

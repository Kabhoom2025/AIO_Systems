import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface VendorBillLineDto {
  id: number;
  ledgerAccountId: number;
  ledgerAccountName: string;
  ledgerAccountCode: string;
  description: string | null;
  amount: number;
  displayOrder: number;
}

export interface CreateVendorBillLineDto {
  ledgerAccountId: number;
  description: string | null;
  amount: number;
  displayOrder: number;
}

export interface VendorBillDto {
  id: number;
  billNumber: string;
  vendorId: number;
  vendorName: string;
  payableLedgerAccountId: number;
  payableLedgerAccountName: string;
  billDate: string;
  dueDate: string;
  status: string;
  ownerId: number;
  ownerName: string;
  postedJournalEntryId: number | null;
  paymentJournalEntryId: number | null;
  lines: VendorBillLineDto[];
  totalAmount: number;
}

export interface CreateVendorBillDto {
  vendorId: number;
  payableLedgerAccountId: number;
  billDate: string;
  dueDate: string;
  ownerId: number;
  lines: CreateVendorBillLineDto[];
}

export interface UpdateVendorBillDto {
  payableLedgerAccountId: number;
  billDate: string;
  dueDate: string;
  ownerId: number;
  lines: CreateVendorBillLineDto[];
}

export interface PayVendorBillDto {
  paymentLedgerAccountId: number;
}

@Injectable({ providedIn: 'root' })
export class VendorBillApiService {
  private billsUrl = `${environment.apiUrl}/vendor-bills`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<VendorBillDto[]> {
    return this.http.get<VendorBillDto[]>(this.billsUrl);
  }

  getById(id: number): Observable<VendorBillDto> {
    return this.http.get<VendorBillDto>(`${this.billsUrl}/${id}`);
  }

  create(dto: CreateVendorBillDto): Observable<VendorBillDto> {
    return this.http.post<VendorBillDto>(this.billsUrl, dto);
  }

  update(id: number, dto: UpdateVendorBillDto): Observable<VendorBillDto> {
    return this.http.put<VendorBillDto>(`${this.billsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.billsUrl}/${id}`);
  }

  approve(id: number): Observable<VendorBillDto> {
    return this.http.post<VendorBillDto>(`${this.billsUrl}/${id}/approve`, {});
  }

  pay(id: number, dto: PayVendorBillDto): Observable<VendorBillDto> {
    return this.http.post<VendorBillDto>(`${this.billsUrl}/${id}/pay`, dto);
  }

  cancel(id: number): Observable<VendorBillDto> {
    return this.http.post<VendorBillDto>(`${this.billsUrl}/${id}/cancel`, {});
  }
}

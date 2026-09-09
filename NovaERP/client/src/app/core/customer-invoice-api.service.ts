import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface CustomerInvoiceLineDto {
  id: number;
  ledgerAccountId: number;
  ledgerAccountName: string;
  ledgerAccountCode: string;
  description: string | null;
  amount: number;
  displayOrder: number;
}

export interface CreateCustomerInvoiceLineDto {
  ledgerAccountId: number;
  description: string | null;
  amount: number;
  displayOrder: number;
}

export interface CustomerInvoiceDto {
  id: number;
  invoiceNumber: string;
  accountId: number;
  accountName: string;
  receivableLedgerAccountId: number;
  receivableLedgerAccountName: string;
  invoiceDate: string;
  dueDate: string;
  status: string;
  ownerId: number;
  ownerName: string;
  postedJournalEntryId: number | null;
  paymentJournalEntryId: number | null;
  lines: CustomerInvoiceLineDto[];
  totalAmount: number;
}

export interface CreateCustomerInvoiceDto {
  accountId: number;
  receivableLedgerAccountId: number;
  invoiceDate: string;
  dueDate: string;
  ownerId: number;
  lines: CreateCustomerInvoiceLineDto[];
}

export interface UpdateCustomerInvoiceDto {
  receivableLedgerAccountId: number;
  invoiceDate: string;
  dueDate: string;
  ownerId: number;
  lines: CreateCustomerInvoiceLineDto[];
}

export interface ReceiveCustomerInvoicePaymentDto {
  paymentLedgerAccountId: number;
}

@Injectable({ providedIn: 'root' })
export class CustomerInvoiceApiService {
  private invoicesUrl = `${environment.apiUrl}/customer-invoices`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<CustomerInvoiceDto[]> {
    return this.http.get<CustomerInvoiceDto[]>(this.invoicesUrl);
  }

  getById(id: number): Observable<CustomerInvoiceDto> {
    return this.http.get<CustomerInvoiceDto>(`${this.invoicesUrl}/${id}`);
  }

  create(dto: CreateCustomerInvoiceDto): Observable<CustomerInvoiceDto> {
    return this.http.post<CustomerInvoiceDto>(this.invoicesUrl, dto);
  }

  update(id: number, dto: UpdateCustomerInvoiceDto): Observable<CustomerInvoiceDto> {
    return this.http.put<CustomerInvoiceDto>(`${this.invoicesUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.invoicesUrl}/${id}`);
  }

  send(id: number): Observable<CustomerInvoiceDto> {
    return this.http.post<CustomerInvoiceDto>(`${this.invoicesUrl}/${id}/send`, {});
  }

  receivePayment(id: number, dto: ReceiveCustomerInvoicePaymentDto): Observable<CustomerInvoiceDto> {
    return this.http.post<CustomerInvoiceDto>(`${this.invoicesUrl}/${id}/receive-payment`, dto);
  }

  cancel(id: number): Observable<CustomerInvoiceDto> {
    return this.http.post<CustomerInvoiceDto>(`${this.invoicesUrl}/${id}/cancel`, {});
  }
}

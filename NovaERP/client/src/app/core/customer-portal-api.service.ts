import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { SalesOrderDto } from './sales-order-api.service';
import { CustomerInvoiceDto } from './customer-invoice-api.service';

export interface MyContactDto {
  id: number;
  firstName: string;
  lastName: string;
  email: string | null;
  phone: string | null;
  title: string | null;
  accountId: number | null;
  accountName: string | null;
}

@Injectable({ providedIn: 'root' })
export class CustomerPortalApiService {
  private myAccountUrl = `${environment.apiUrl}/my-account`;

  constructor(private http: HttpClient) {}

  getMyProfile(): Observable<MyContactDto> {
    return this.http.get<MyContactDto>(`${this.myAccountUrl}/profile`);
  }

  getMyOrders(): Observable<SalesOrderDto[]> {
    return this.http.get<SalesOrderDto[]>(`${this.myAccountUrl}/orders`);
  }

  getMyInvoices(): Observable<CustomerInvoiceDto[]> {
    return this.http.get<CustomerInvoiceDto[]>(`${this.myAccountUrl}/invoices`);
  }
}

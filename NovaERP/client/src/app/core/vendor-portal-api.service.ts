import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PurchaseOrderDto } from './purchase-order-api.service';
import { VendorBillDto } from './vendor-bill-api.service';

export interface MyVendorDto {
  id: number;
  name: string;
  category: string | null;
  contactEmail: string | null;
  contactPhone: string | null;
  address: string | null;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class VendorPortalApiService {
  private vendorPortalUrl = `${environment.apiUrl}/vendor-portal`;

  constructor(private http: HttpClient) {}

  getMyProfile(): Observable<MyVendorDto> {
    return this.http.get<MyVendorDto>(`${this.vendorPortalUrl}/profile`);
  }

  getMyPurchaseOrders(): Observable<PurchaseOrderDto[]> {
    return this.http.get<PurchaseOrderDto[]>(`${this.vendorPortalUrl}/purchase-orders`);
  }

  getMyBills(): Observable<VendorBillDto[]> {
    return this.http.get<VendorBillDto[]>(`${this.vendorPortalUrl}/bills`);
  }
}

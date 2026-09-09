import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response.model';
import {
  Customer,
  CreateCustomerRequest,
  UpdateCustomerRequest,
  AdjustPointsRequest,
  PointsTransaction,
  SavedAddress,
  AddSavedAddressRequest,
  CustomerOrderSummary,
} from '../models/customer.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CustomerService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/customers`;

  getAll(): Observable<ApiResponse<Customer[]>> {
    return this.http.get<ApiResponse<Customer[]>>(this.apiUrl);
  }

  getById(id: number): Observable<ApiResponse<Customer>> {
    return this.http.get<ApiResponse<Customer>>(`${this.apiUrl}/${id}`);
  }

  getByPhone(phone: string): Observable<ApiResponse<Customer>> {
    return this.http.get<ApiResponse<Customer>>(
      `${this.apiUrl}/phone/${encodeURIComponent(phone)}`
    );
  }

  getTransactions(id: number): Observable<ApiResponse<PointsTransaction[]>> {
    return this.http.get<ApiResponse<PointsTransaction[]>>(
      `${this.apiUrl}/${id}/transactions`
    );
  }

  getOrders(id: number): Observable<ApiResponse<CustomerOrderSummary[]>> {
    return this.http.get<ApiResponse<CustomerOrderSummary[]>>(
      `${this.apiUrl}/${id}/orders`
    );
  }

  getAddresses(id: number): Observable<ApiResponse<SavedAddress[]>> {
    return this.http.get<ApiResponse<SavedAddress[]>>(
      `${this.apiUrl}/${id}/addresses`
    );
  }

  create(dto: CreateCustomerRequest): Observable<ApiResponse<Customer>> {
    return this.http.post<ApiResponse<Customer>>(this.apiUrl, dto);
  }

  update(id: number, dto: UpdateCustomerRequest): Observable<ApiResponse<Customer>> {
    return this.http.put<ApiResponse<Customer>>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.apiUrl}/${id}`);
  }

  adjustPoints(id: number, dto: AdjustPointsRequest): Observable<ApiResponse<Customer>> {
    return this.http.post<ApiResponse<Customer>>(
      `${this.apiUrl}/${id}/adjust-points`,
      dto
    );
  }

  addAddress(id: number, dto: AddSavedAddressRequest): Observable<ApiResponse<SavedAddress>> {
    return this.http.post<ApiResponse<SavedAddress>>(
      `${this.apiUrl}/${id}/addresses`,
      dto
    );
  }

  deleteAddress(customerId: number, addressId: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(
      `${this.apiUrl}/${customerId}/addresses/${addressId}`
    );
  }
}

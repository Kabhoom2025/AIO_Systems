import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface WarehouseDto {
  id: number;
  branchId: number;
  branchName: string;
  name: string;
  code: string;
  contactName: string | null;
  email: string | null;
  phone: string | null;
  address: string | null;
  addressLine2: string | null;
  city: string | null;
  state: string | null;
  postalCode: string | null;
  country: string | null;
  taxType: string | null;
  taxCountry: string | null;
  taxId: string | null;
  isActive: boolean;
}

export interface CreateWarehouseDto {
  branchId: number;
  name: string;
  code: string;
  contactName: string | null;
  email: string | null;
  phone: string | null;
  address: string | null;
  addressLine2: string | null;
  city: string | null;
  state: string | null;
  postalCode: string | null;
  country: string | null;
  taxType: string | null;
  taxCountry: string | null;
  taxId: string | null;
  isActive: boolean;
}

export interface UpdateWarehouseDto {
  branchId: number;
  name: string;
  contactName: string | null;
  email: string | null;
  phone: string | null;
  address: string | null;
  addressLine2: string | null;
  city: string | null;
  state: string | null;
  postalCode: string | null;
  country: string | null;
  taxType: string | null;
  taxCountry: string | null;
  taxId: string | null;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class WarehouseApiService {
  private warehousesUrl = `${environment.apiUrl}/warehouses`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<WarehouseDto[]> {
    return this.http.get<WarehouseDto[]>(this.warehousesUrl);
  }

  getById(id: number): Observable<WarehouseDto> {
    return this.http.get<WarehouseDto>(`${this.warehousesUrl}/${id}`);
  }

  create(dto: CreateWarehouseDto): Observable<WarehouseDto> {
    return this.http.post<WarehouseDto>(this.warehousesUrl, dto);
  }

  update(id: number, dto: UpdateWarehouseDto): Observable<WarehouseDto> {
    return this.http.put<WarehouseDto>(`${this.warehousesUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.warehousesUrl}/${id}`);
  }
}

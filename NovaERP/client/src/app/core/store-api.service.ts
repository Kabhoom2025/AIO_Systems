import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface StoreDto {
  id: number;
  branchId: number;
  branchName: string;
  warehouseId: number;
  warehouseName: string;
  name: string;
  code: string;
  address: string | null;
  isActive: boolean;
}

export interface CreateStoreDto {
  branchId: number;
  warehouseId: number;
  name: string;
  code: string;
  address: string | null;
  isActive: boolean;
}

export interface UpdateStoreDto {
  branchId: number;
  warehouseId: number;
  name: string;
  address: string | null;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class StoreApiService {
  private storesUrl = `${environment.apiUrl}/stores`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<StoreDto[]> {
    return this.http.get<StoreDto[]>(this.storesUrl);
  }

  getById(id: number): Observable<StoreDto> {
    return this.http.get<StoreDto>(`${this.storesUrl}/${id}`);
  }

  create(dto: CreateStoreDto): Observable<StoreDto> {
    return this.http.post<StoreDto>(this.storesUrl, dto);
  }

  update(id: number, dto: UpdateStoreDto): Observable<StoreDto> {
    return this.http.put<StoreDto>(`${this.storesUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.storesUrl}/${id}`);
  }
}

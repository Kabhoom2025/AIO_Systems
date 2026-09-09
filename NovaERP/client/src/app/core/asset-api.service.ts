import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AssetDto {
  id: number;
  assetCode: string;
  name: string;
  categoryId: number;
  categoryName: string;
  serialNumber: string | null;

  purchaseDate: string;
  purchaseCost: number | null;
  warrantyExpiryDate: string | null;

  assignedToId: number | null;
  assignedToName: string | null;
  status: string;
}

export interface CreateAssetDto {
  name: string;
  categoryId: number;
  serialNumber: string | null;

  purchaseDate: string;
  purchaseCost: number | null;
  warrantyExpiryDate: string | null;
}

export interface UpdateAssetDto {
  name: string;
  categoryId: number;
  serialNumber: string | null;

  purchaseDate: string;
  purchaseCost: number | null;
  warrantyExpiryDate: string | null;
}

export interface AssignAssetDto {
  employeeId: number;
}

@Injectable({ providedIn: 'root' })
export class AssetApiService {
  private assetsUrl = `${environment.apiUrl}/assets`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<AssetDto[]> {
    return this.http.get<AssetDto[]>(this.assetsUrl);
  }

  getById(id: number): Observable<AssetDto> {
    return this.http.get<AssetDto>(`${this.assetsUrl}/${id}`);
  }

  create(dto: CreateAssetDto): Observable<AssetDto> {
    return this.http.post<AssetDto>(this.assetsUrl, dto);
  }

  update(id: number, dto: UpdateAssetDto): Observable<AssetDto> {
    return this.http.put<AssetDto>(`${this.assetsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.assetsUrl}/${id}`);
  }

  assign(id: number, dto: AssignAssetDto): Observable<AssetDto> {
    return this.http.post<AssetDto>(`${this.assetsUrl}/${id}/assign`, dto);
  }

  unassign(id: number): Observable<AssetDto> {
    return this.http.post<AssetDto>(`${this.assetsUrl}/${id}/unassign`, {});
  }

  retire(id: number): Observable<AssetDto> {
    return this.http.post<AssetDto>(`${this.assetsUrl}/${id}/retire`, {});
  }
}

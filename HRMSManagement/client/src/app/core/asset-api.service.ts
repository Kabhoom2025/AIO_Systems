import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AssetDto {
  id: number;
  assetTag: string;
  name: string;
  category: string;
  serialNumber: string | null;
  purchaseDate: string | null;
  purchaseCost: number | null;
  warrantyUntil: string | null;
  condition: string;
  status: string;
  notes: string | null;
  branchId: number | null;
  branchName: string | null;
  currentHolderEmployeeId: number | null;
  currentHolderName: string | null;
}

export interface AllocationDto {
  id: number;
  employeeId: number;
  employeeName: string;
  allocatedDate: string;
  returnedDate: string | null;
  returnCondition: string | null;
  notes: string | null;
  allocatedByUserId: number | null;
  allocatedByName: string | null;
}

export interface AssetDetailDto extends AssetDto {
  allocations: AllocationDto[];
}

export interface CreateAssetDto {
  name: string;
  category: string;
  serialNumber?: string | null;
  purchaseDate?: string | null;
  purchaseCost?: number | null;
  warrantyUntil?: string | null;
  condition: string;
  branchId?: number | null;
  notes?: string | null;
}

export interface UpdateAssetDto extends CreateAssetDto {
  status: string;
}

export interface AllocateAssetDto {
  employeeId: number;
  notes?: string | null;
}

export interface ReturnAssetDto {
  returnCondition?: string | null;
  notes?: string | null;
}

@Injectable({ providedIn: 'root' })
export class AssetApiService {
  private base = `${environment.apiUrl}/assets`;

  constructor(private http: HttpClient) {}

  getAll(category?: string | null, status?: string | null): Observable<AssetDto[]> {
    let params = new HttpParams();
    if (category) params = params.set('category', category);
    if (status) params = params.set('status', status);
    return this.http.get<AssetDto[]>(this.base, { params });
  }

  getMy(): Observable<AssetDto[]> {
    return this.http.get<AssetDto[]>(`${this.base}/my`);
  }

  getById(id: number): Observable<AssetDetailDto> {
    return this.http.get<AssetDetailDto>(`${this.base}/${id}`);
  }

  create(dto: CreateAssetDto): Observable<AssetDto> {
    return this.http.post<AssetDto>(this.base, dto);
  }

  update(id: number, dto: UpdateAssetDto): Observable<AssetDto> {
    return this.http.put<AssetDto>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  allocate(id: number, dto: AllocateAssetDto): Observable<AssetDto> {
    return this.http.post<AssetDto>(`${this.base}/${id}/allocate`, dto);
  }

  return(id: number, dto: ReturnAssetDto): Observable<AssetDto> {
    return this.http.post<AssetDto>(`${this.base}/${id}/return`, dto);
  }
}

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface BomComponentDto {
  id: number;
  componentProductId: number;
  componentProductName: string;
  componentProductSku: string;
  quantity: number;
  displayOrder: number;
}

export interface CreateBomComponentDto {
  componentProductId: number;
  quantity: number;
  displayOrder: number;
}

export interface BillOfMaterialDto {
  id: number;
  productId: number;
  productName: string;
  productSku: string;
  isActive: boolean;
  components: BomComponentDto[];
}

export interface CreateBillOfMaterialDto {
  productId: number;
  isActive: boolean;
  components: CreateBomComponentDto[];
}

export interface UpdateBillOfMaterialDto {
  isActive: boolean;
  components: CreateBomComponentDto[];
}

@Injectable({ providedIn: 'root' })
export class BillOfMaterialApiService {
  private bomsUrl = `${environment.apiUrl}/bill-of-materials`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<BillOfMaterialDto[]> {
    return this.http.get<BillOfMaterialDto[]>(this.bomsUrl);
  }

  getById(id: number): Observable<BillOfMaterialDto> {
    return this.http.get<BillOfMaterialDto>(`${this.bomsUrl}/${id}`);
  }

  create(dto: CreateBillOfMaterialDto): Observable<BillOfMaterialDto> {
    return this.http.post<BillOfMaterialDto>(this.bomsUrl, dto);
  }

  update(id: number, dto: UpdateBillOfMaterialDto): Observable<BillOfMaterialDto> {
    return this.http.put<BillOfMaterialDto>(`${this.bomsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.bomsUrl}/${id}`);
  }
}

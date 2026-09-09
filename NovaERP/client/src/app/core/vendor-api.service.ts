import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface VendorDto {
  id: number;
  name: string;
  category: string | null;
  contactEmail: string | null;
  contactPhone: string | null;
  address: string | null;
  isActive: boolean;
  ownerId: number;
  ownerName: string;
}

export interface CreateVendorDto {
  name: string;
  category: string | null;
  contactEmail: string | null;
  contactPhone: string | null;
  address: string | null;
  isActive: boolean;
  ownerId: number;
}

export interface UpdateVendorDto {
  name: string;
  category: string | null;
  contactEmail: string | null;
  contactPhone: string | null;
  address: string | null;
  isActive: boolean;
  ownerId: number;
}

@Injectable({ providedIn: 'root' })
export class VendorApiService {
  private vendorsUrl = `${environment.apiUrl}/vendors`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<VendorDto[]> {
    return this.http.get<VendorDto[]>(this.vendorsUrl);
  }

  getById(id: number): Observable<VendorDto> {
    return this.http.get<VendorDto>(`${this.vendorsUrl}/${id}`);
  }

  create(dto: CreateVendorDto): Observable<VendorDto> {
    return this.http.post<VendorDto>(this.vendorsUrl, dto);
  }

  update(id: number, dto: UpdateVendorDto): Observable<VendorDto> {
    return this.http.put<VendorDto>(`${this.vendorsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.vendorsUrl}/${id}`);
  }
}

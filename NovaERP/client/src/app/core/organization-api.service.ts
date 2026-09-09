import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface OrganizationDto {
  id: number;
  name: string;
  code: string;
  legalName?: string | null;
  address?: string | null;
  phone?: string | null;
  email?: string | null;
  website?: string | null;
  taxNumber?: string | null;
  timezone: string;
  currency: string;
  logoUrl?: string | null;
  isActive: boolean;
}

export interface UpdateOrganizationDto {
  name: string;
  legalName?: string | null;
  address?: string | null;
  phone?: string | null;
  email?: string | null;
  website?: string | null;
  taxNumber?: string | null;
  timezone: string;
  currency: string;
  logoUrl?: string | null;
}

@Injectable({ providedIn: 'root' })
export class OrganizationApiService {
  private orgUrl = `${environment.apiUrl}/organization`;

  constructor(private http: HttpClient) {}

  getProfile(): Observable<OrganizationDto> {
    return this.http.get<OrganizationDto>(this.orgUrl);
  }

  updateProfile(dto: UpdateOrganizationDto): Observable<OrganizationDto> {
    return this.http.put<OrganizationDto>(this.orgUrl, dto);
  }
}

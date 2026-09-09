import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface TaxComponentDto {
  id: number;
  name: string;
  ratePercent: number;
  displayOrder: number;
}

export interface CreateTaxComponentDto {
  name: string;
  ratePercent: number;
  displayOrder: number;
}

export interface TaxCodeDto {
  id: number;
  code: string;
  name: string;
  isActive: boolean;
  components: TaxComponentDto[];
  totalRatePercent: number;
}

export interface CreateTaxCodeDto {
  code: string;
  name: string;
  isActive: boolean;
  components: CreateTaxComponentDto[];
}

export interface UpdateTaxCodeDto {
  name: string;
  isActive: boolean;
  components: CreateTaxComponentDto[];
}

@Injectable({ providedIn: 'root' })
export class TaxCodeApiService {
  private taxCodesUrl = `${environment.apiUrl}/tax-codes`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<TaxCodeDto[]> {
    return this.http.get<TaxCodeDto[]>(this.taxCodesUrl);
  }

  getById(id: number): Observable<TaxCodeDto> {
    return this.http.get<TaxCodeDto>(`${this.taxCodesUrl}/${id}`);
  }

  create(dto: CreateTaxCodeDto): Observable<TaxCodeDto> {
    return this.http.post<TaxCodeDto>(this.taxCodesUrl, dto);
  }

  update(id: number, dto: UpdateTaxCodeDto): Observable<TaxCodeDto> {
    return this.http.put<TaxCodeDto>(`${this.taxCodesUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.taxCodesUrl}/${id}`);
  }
}

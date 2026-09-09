import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ExchangeRateDto {
  id: number;
  fromCurrencyCode: string;
  toCurrencyCode: string;
  rate: number;
  effectiveDate: string;
}

export interface CreateExchangeRateDto {
  fromCurrencyCode: string;
  toCurrencyCode: string;
  rate: number;
  effectiveDate: string;
}

export interface UpdateExchangeRateDto {
  rate: number;
  effectiveDate: string;
}

@Injectable({ providedIn: 'root' })
export class ExchangeRateApiService {
  private ratesUrl = `${environment.apiUrl}/exchange-rates`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ExchangeRateDto[]> {
    return this.http.get<ExchangeRateDto[]>(this.ratesUrl);
  }

  getById(id: number): Observable<ExchangeRateDto> {
    return this.http.get<ExchangeRateDto>(`${this.ratesUrl}/${id}`);
  }

  create(dto: CreateExchangeRateDto): Observable<ExchangeRateDto> {
    return this.http.post<ExchangeRateDto>(this.ratesUrl, dto);
  }

  update(id: number, dto: UpdateExchangeRateDto): Observable<ExchangeRateDto> {
    return this.http.put<ExchangeRateDto>(`${this.ratesUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.ratesUrl}/${id}`);
  }
}

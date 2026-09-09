import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface CurrencyDto {
  id: number;
  code: string;
  name: string;
  symbol: string;
  decimalPlaces: number;
  isActive: boolean;
}

export interface CreateCurrencyDto {
  code: string;
  name: string;
  symbol: string;
  decimalPlaces: number;
}

export interface UpdateCurrencyDto {
  name: string;
  symbol: string;
  decimalPlaces: number;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class CurrencyApiService {
  private currenciesUrl = `${environment.apiUrl}/currencies`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<CurrencyDto[]> {
    return this.http.get<CurrencyDto[]>(this.currenciesUrl);
  }

  create(dto: CreateCurrencyDto): Observable<CurrencyDto> {
    return this.http.post<CurrencyDto>(this.currenciesUrl, dto);
  }

  update(code: string, dto: UpdateCurrencyDto): Observable<CurrencyDto> {
    return this.http.put<CurrencyDto>(`${this.currenciesUrl}/${code}`, dto);
  }
}

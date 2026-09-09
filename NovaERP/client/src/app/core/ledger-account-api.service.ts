import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface LedgerAccountDto {
  id: number;
  code: string;
  name: string;
  type: string;
  isActive: boolean;
  balance: number;
}

export interface CreateLedgerAccountDto {
  code: string;
  name: string;
  type: string;
  isActive: boolean;
}

export interface UpdateLedgerAccountDto {
  name: string;
  type: string;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class LedgerAccountApiService {
  private accountsUrl = `${environment.apiUrl}/ledger-accounts`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<LedgerAccountDto[]> {
    return this.http.get<LedgerAccountDto[]>(this.accountsUrl);
  }

  getById(id: number): Observable<LedgerAccountDto> {
    return this.http.get<LedgerAccountDto>(`${this.accountsUrl}/${id}`);
  }

  create(dto: CreateLedgerAccountDto): Observable<LedgerAccountDto> {
    return this.http.post<LedgerAccountDto>(this.accountsUrl, dto);
  }

  update(id: number, dto: UpdateLedgerAccountDto): Observable<LedgerAccountDto> {
    return this.http.put<LedgerAccountDto>(`${this.accountsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.accountsUrl}/${id}`);
  }
}

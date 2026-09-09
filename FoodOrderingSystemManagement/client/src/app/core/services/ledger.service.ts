import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { LedgerEntry, CreateLedgerEntryRequest, LedgerSummary } from '../models/ledger.model';

@Injectable({ providedIn: 'root' })
export class LedgerService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/ledger`;

  getEntries(from: string, to: string): Observable<ApiResponse<LedgerEntry[]>> {
    const params = new HttpParams().set('from', from).set('to', to);
    return this.http.get<ApiResponse<LedgerEntry[]>>(this.apiUrl, { params });
  }

  getSummary(date: string): Observable<ApiResponse<LedgerSummary>> {
    const params = new HttpParams().set('date', date);
    return this.http.get<ApiResponse<LedgerSummary>>(`${this.apiUrl}/summary`, { params });
  }

  create(payload: CreateLedgerEntryRequest): Observable<ApiResponse<LedgerEntry>> {
    return this.http.post<ApiResponse<LedgerEntry>>(this.apiUrl, payload);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.apiUrl}/${id}`);
  }
}

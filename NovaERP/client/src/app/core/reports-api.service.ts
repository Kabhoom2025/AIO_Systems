import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface TrialBalanceLineDto {
  code: string;
  name: string;
  type: string;
  debit: number;
  credit: number;
}

export interface TrialBalanceDto {
  lines: TrialBalanceLineDto[];
  totalDebit: number;
  totalCredit: number;
}

export interface StatusSummaryRowDto {
  status: string;
  count: number;
  total: number;
}

export interface StatusSummaryReportDto {
  rows: StatusSummaryRowDto[];
  grandTotal: number;
}

@Injectable({ providedIn: 'root' })
export class ReportsApiService {
  private reportsUrl = `${environment.apiUrl}/reports`;

  constructor(private http: HttpClient) {}

  getTrialBalance(): Observable<TrialBalanceDto> {
    return this.http.get<TrialBalanceDto>(`${this.reportsUrl}/trial-balance`);
  }

  getSalesOrderSummary(): Observable<StatusSummaryReportDto> {
    return this.http.get<StatusSummaryReportDto>(`${this.reportsUrl}/sales-order-summary`);
  }

  getPurchaseOrderSummary(): Observable<StatusSummaryReportDto> {
    return this.http.get<StatusSummaryReportDto>(`${this.reportsUrl}/purchase-order-summary`);
  }
}

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PayRunLineDto {
  id: number;
  employeeId: number;
  employeeName: string;
  employeeCode: string;

  basicSalary: number;
  hra: number;
  otherAllowances: number;
  deductions: number;
  grossPay: number;
  netPay: number;
}

export interface PayRunDto {
  id: number;
  runNumber: string;
  periodMonth: number;
  periodYear: number;
  expenseLedgerAccountId: number;
  expenseLedgerAccountName: string;
  deductionsPayableLedgerAccountId: number;
  deductionsPayableLedgerAccountName: string;
  status: string;
  ownerId: number;
  ownerName: string;
  postedJournalEntryId: number | null;

  lines: PayRunLineDto[];

  totalGrossPay: number;
  totalDeductions: number;
  totalNetPay: number;
}

export interface CreatePayRunDto {
  periodMonth: number;
  periodYear: number;
  expenseLedgerAccountId: number;
  deductionsPayableLedgerAccountId: number;
  ownerId: number;
}

export interface UpdatePayRunDto {
  periodMonth: number;
  periodYear: number;
  expenseLedgerAccountId: number;
  deductionsPayableLedgerAccountId: number;
  ownerId: number;
}

export interface PayPayRunDto {
  paymentLedgerAccountId: number;
}

@Injectable({ providedIn: 'root' })
export class PayRunApiService {
  private payRunsUrl = `${environment.apiUrl}/pay-runs`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<PayRunDto[]> {
    return this.http.get<PayRunDto[]>(this.payRunsUrl);
  }

  getById(id: number): Observable<PayRunDto> {
    return this.http.get<PayRunDto>(`${this.payRunsUrl}/${id}`);
  }

  create(dto: CreatePayRunDto): Observable<PayRunDto> {
    return this.http.post<PayRunDto>(this.payRunsUrl, dto);
  }

  update(id: number, dto: UpdatePayRunDto): Observable<PayRunDto> {
    return this.http.put<PayRunDto>(`${this.payRunsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.payRunsUrl}/${id}`);
  }

  process(id: number): Observable<PayRunDto> {
    return this.http.post<PayRunDto>(`${this.payRunsUrl}/${id}/process`, {});
  }

  pay(id: number, dto: PayPayRunDto): Observable<PayRunDto> {
    return this.http.post<PayRunDto>(`${this.payRunsUrl}/${id}/pay`, dto);
  }

  cancel(id: number): Observable<PayRunDto> {
    return this.http.post<PayRunDto>(`${this.payRunsUrl}/${id}/cancel`, {});
  }
}

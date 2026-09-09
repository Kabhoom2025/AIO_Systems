import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ExpenseClaimDto {
  id: number;
  employeeId: number;
  employeeName: string;
  employeeCode: string;
  category: string;
  claimDate: string;
  amount: number;
  currency: string;
  description: string;
  receiptUrl: string | null;
  status: string;
  reviewedByUserId: number | null;
  reviewedByName: string | null;
  reviewedAt: string | null;
  reviewNotes: string | null;
  reimbursedAt: string | null;
}

export interface CreateExpenseClaimDto {
  category: string;
  claimDate: string;
  amount: number;
  currency: string;
  description: string;
  receiptUrl?: string | null;
}

export interface ReviewExpenseDto {
  notes?: string | null;
}

export interface ExpenseSummaryDto {
  pending: number;
  approved: number;
  reimbursed: number;
  rejected: number;
  totalPendingAmount: number;
  totalReimbursedAmount: number;
}

@Injectable({ providedIn: 'root' })
export class ExpenseApiService {
  private base = `${environment.apiUrl}/expenses`;

  constructor(private http: HttpClient) {}

  create(dto: CreateExpenseClaimDto): Observable<ExpenseClaimDto> {
    return this.http.post<ExpenseClaimDto>(this.base, dto);
  }

  my(): Observable<ExpenseClaimDto[]> {
    return this.http.get<ExpenseClaimDto[]>(`${this.base}/my`);
  }

  getAll(status?: string | null): Observable<ExpenseClaimDto[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    return this.http.get<ExpenseClaimDto[]>(this.base, { params });
  }

  getById(id: number): Observable<ExpenseClaimDto> {
    return this.http.get<ExpenseClaimDto>(`${this.base}/${id}`);
  }

  summary(): Observable<ExpenseSummaryDto> {
    return this.http.get<ExpenseSummaryDto>(`${this.base}/summary`);
  }

  approve(id: number, dto: ReviewExpenseDto): Observable<ExpenseClaimDto> {
    return this.http.post<ExpenseClaimDto>(`${this.base}/${id}/approve`, dto);
  }

  reject(id: number, dto: ReviewExpenseDto): Observable<ExpenseClaimDto> {
    return this.http.post<ExpenseClaimDto>(`${this.base}/${id}/reject`, dto);
  }

  reimburse(id: number): Observable<ExpenseClaimDto> {
    return this.http.post<ExpenseClaimDto>(`${this.base}/${id}/reimburse`, {});
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}

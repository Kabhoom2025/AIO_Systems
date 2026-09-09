import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface BankStatementLineDto {
  id: number;
  transactionDate: string;
  description: string | null;
  amount: number;
  matchedJournalEntryLineId: number | null;
  displayOrder: number;
  isMatched: boolean;
}

export interface CreateBankStatementLineDto {
  transactionDate: string;
  description: string | null;
  amount: number;
  displayOrder: number;
}

export interface BankReconciliationDto {
  id: number;
  ledgerAccountId: number;
  ledgerAccountName: string;
  statementDate: string;
  statementEndingBalance: number;
  status: string;
  ownerId: number;
  ownerName: string;
  lines: BankStatementLineDto[];
  bookBalance: number;
  difference: number;
  unmatchedCount: number;
}

export interface CreateBankReconciliationDto {
  ledgerAccountId: number;
  statementDate: string;
  statementEndingBalance: number;
  ownerId: number;
  lines: CreateBankStatementLineDto[];
}

export interface UpdateBankReconciliationDto {
  statementDate: string;
  statementEndingBalance: number;
  ownerId: number;
  lines: CreateBankStatementLineDto[];
}

export interface MatchBankStatementLineDto {
  journalEntryLineId: number;
}

export interface MatchCandidateDto {
  journalEntryLineId: number;
  journalEntryNumber: string;
  entryDate: string;
  description: string | null;
  amount: number;
}

@Injectable({ providedIn: 'root' })
export class BankReconciliationApiService {
  private reconciliationsUrl = `${environment.apiUrl}/bank-reconciliations`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<BankReconciliationDto[]> {
    return this.http.get<BankReconciliationDto[]>(this.reconciliationsUrl);
  }

  getById(id: number): Observable<BankReconciliationDto> {
    return this.http.get<BankReconciliationDto>(`${this.reconciliationsUrl}/${id}`);
  }

  getMatchCandidates(ledgerAccountId: number): Observable<MatchCandidateDto[]> {
    return this.http.get<MatchCandidateDto[]>(`${this.reconciliationsUrl}/match-candidates?ledgerAccountId=${ledgerAccountId}`);
  }

  create(dto: CreateBankReconciliationDto): Observable<BankReconciliationDto> {
    return this.http.post<BankReconciliationDto>(this.reconciliationsUrl, dto);
  }

  update(id: number, dto: UpdateBankReconciliationDto): Observable<BankReconciliationDto> {
    return this.http.put<BankReconciliationDto>(`${this.reconciliationsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.reconciliationsUrl}/${id}`);
  }

  matchLine(reconciliationId: number, lineId: number, dto: MatchBankStatementLineDto): Observable<BankReconciliationDto> {
    return this.http.post<BankReconciliationDto>(`${this.reconciliationsUrl}/${reconciliationId}/lines/${lineId}/match`, dto);
  }

  unmatchLine(reconciliationId: number, lineId: number): Observable<BankReconciliationDto> {
    return this.http.post<BankReconciliationDto>(`${this.reconciliationsUrl}/${reconciliationId}/lines/${lineId}/unmatch`, {});
  }

  complete(id: number): Observable<BankReconciliationDto> {
    return this.http.post<BankReconciliationDto>(`${this.reconciliationsUrl}/${id}/complete`, {});
  }
}

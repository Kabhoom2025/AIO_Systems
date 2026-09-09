import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface JournalEntryLineDto {
  id: number;
  ledgerAccountId: number;
  ledgerAccountName: string;
  ledgerAccountCode: string;
  debit: number;
  credit: number;
  description: string | null;
  displayOrder: number;
}

export interface CreateJournalEntryLineDto {
  ledgerAccountId: number;
  debit: number;
  credit: number;
  description: string | null;
  displayOrder: number;
}

export interface JournalEntryDto {
  id: number;
  entryNumber: string;
  entryDate: string;
  description: string | null;
  status: string;
  ownerId: number;
  ownerName: string;
  lines: JournalEntryLineDto[];
  totalDebit: number;
  totalCredit: number;
}

export interface CreateJournalEntryDto {
  entryDate: string;
  description: string | null;
  ownerId: number;
  lines: CreateJournalEntryLineDto[];
}

export interface UpdateJournalEntryDto {
  entryDate: string;
  description: string | null;
  ownerId: number;
  lines: CreateJournalEntryLineDto[];
}

@Injectable({ providedIn: 'root' })
export class JournalEntryApiService {
  private entriesUrl = `${environment.apiUrl}/journal-entries`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<JournalEntryDto[]> {
    return this.http.get<JournalEntryDto[]>(this.entriesUrl);
  }

  getById(id: number): Observable<JournalEntryDto> {
    return this.http.get<JournalEntryDto>(`${this.entriesUrl}/${id}`);
  }

  create(dto: CreateJournalEntryDto): Observable<JournalEntryDto> {
    return this.http.post<JournalEntryDto>(this.entriesUrl, dto);
  }

  update(id: number, dto: UpdateJournalEntryDto): Observable<JournalEntryDto> {
    return this.http.put<JournalEntryDto>(`${this.entriesUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.entriesUrl}/${id}`);
  }

  post(id: number): Observable<JournalEntryDto> {
    return this.http.post<JournalEntryDto>(`${this.entriesUrl}/${id}/post`, {});
  }

  void(id: number): Observable<JournalEntryDto> {
    return this.http.post<JournalEntryDto>(`${this.entriesUrl}/${id}/void`, {});
  }
}

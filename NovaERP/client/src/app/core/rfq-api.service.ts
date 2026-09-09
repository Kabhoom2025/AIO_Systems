import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface RfqItemDto {
  id: number;
  itemName: string;
  quantity: number;
  displayOrder: number;
}

export interface CreateRfqItemDto {
  itemName: string;
  quantity: number;
  displayOrder: number;
}

export interface RfqVendorQuoteDto {
  id: number;
  vendorId: number;
  vendorName: string;
  quotedAmount: number | null;
  notes: string | null;
  respondedDate: string | null;
}

export interface RfqRequestDto {
  id: number;
  rfqNumber: string;
  title: string;
  description: string | null;
  status: string;
  issueDate: string;
  responseDeadline: string | null;
  ownerId: number;
  ownerName: string;
  winningVendorId: number | null;
  winningVendorName: string | null;
  items: RfqItemDto[];
  quotes: RfqVendorQuoteDto[];
}

export interface CreateRfqRequestDto {
  title: string;
  description: string | null;
  issueDate: string;
  responseDeadline: string | null;
  ownerId: number;
  items: CreateRfqItemDto[];
  invitedVendorIds: number[];
}

export interface UpdateRfqRequestDto {
  title: string;
  description: string | null;
  issueDate: string;
  responseDeadline: string | null;
  ownerId: number;
  items: CreateRfqItemDto[];
  invitedVendorIds: number[];
}

export interface RecordRfqQuoteDto {
  vendorId: number;
  quotedAmount: number;
  notes: string | null;
}

export interface CloseRfqRequestDto {
  winningVendorId: number | null;
}

@Injectable({ providedIn: 'root' })
export class RfqApiService {
  private rfqsUrl = `${environment.apiUrl}/rfqs`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<RfqRequestDto[]> {
    return this.http.get<RfqRequestDto[]>(this.rfqsUrl);
  }

  getById(id: number): Observable<RfqRequestDto> {
    return this.http.get<RfqRequestDto>(`${this.rfqsUrl}/${id}`);
  }

  create(dto: CreateRfqRequestDto): Observable<RfqRequestDto> {
    return this.http.post<RfqRequestDto>(this.rfqsUrl, dto);
  }

  update(id: number, dto: UpdateRfqRequestDto): Observable<RfqRequestDto> {
    return this.http.put<RfqRequestDto>(`${this.rfqsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.rfqsUrl}/${id}`);
  }

  send(id: number): Observable<RfqRequestDto> {
    return this.http.post<RfqRequestDto>(`${this.rfqsUrl}/${id}/send`, {});
  }

  recordQuote(id: number, dto: RecordRfqQuoteDto): Observable<RfqRequestDto> {
    return this.http.post<RfqRequestDto>(`${this.rfqsUrl}/${id}/record-quote`, dto);
  }

  close(id: number, dto: CloseRfqRequestDto): Observable<RfqRequestDto> {
    return this.http.post<RfqRequestDto>(`${this.rfqsUrl}/${id}/close`, dto);
  }

  cancel(id: number): Observable<RfqRequestDto> {
    return this.http.post<RfqRequestDto>(`${this.rfqsUrl}/${id}/cancel`, {});
  }
}

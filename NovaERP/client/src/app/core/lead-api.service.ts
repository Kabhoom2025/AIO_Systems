import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface LeadDto {
  id: number;
  name: string;
  companyName: string | null;
  email: string | null;
  phone: string | null;
  source: string | null;
  status: string;
  ownerId: number;
  ownerName: string;
  convertedAccountId: number | null;
  convertedAccountName: string | null;
  convertedDate: string | null;
}

export interface CreateLeadDto {
  name: string;
  companyName: string | null;
  email: string | null;
  phone: string | null;
  source: string | null;
  ownerId: number;
}

export interface UpdateLeadDto {
  name: string;
  companyName: string | null;
  email: string | null;
  phone: string | null;
  source: string | null;
  status: string;
  ownerId: number;
}

export interface ConvertLeadDto {
  accountName: string | null;
  createOpportunity: boolean;
  opportunityName: string | null;
  opportunityAmount: number | null;
}

export interface ConvertLeadResultDto {
  leadId: number;
  accountId: number;
  contactId: number;
  opportunityId: number | null;
}

@Injectable({ providedIn: 'root' })
export class LeadApiService {
  private leadsUrl = `${environment.apiUrl}/leads`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<LeadDto[]> {
    return this.http.get<LeadDto[]>(this.leadsUrl);
  }

  getById(id: number): Observable<LeadDto> {
    return this.http.get<LeadDto>(`${this.leadsUrl}/${id}`);
  }

  create(dto: CreateLeadDto): Observable<LeadDto> {
    return this.http.post<LeadDto>(this.leadsUrl, dto);
  }

  update(id: number, dto: UpdateLeadDto): Observable<LeadDto> {
    return this.http.put<LeadDto>(`${this.leadsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.leadsUrl}/${id}`);
  }

  convert(id: number, dto: ConvertLeadDto): Observable<ConvertLeadResultDto> {
    return this.http.post<ConvertLeadResultDto>(`${this.leadsUrl}/${id}/convert`, dto);
  }
}

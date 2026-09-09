import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface OpportunityDto {
  id: number;
  accountId: number;
  accountName: string;
  name: string;
  amount: number;
  stage: string;
  closeDate: string | null;
  ownerId: number;
  ownerName: string;
}

export interface CreateOpportunityDto {
  accountId: number;
  name: string;
  amount: number;
  stage: string;
  closeDate: string | null;
  ownerId: number;
}

export interface UpdateOpportunityDto {
  name: string;
  amount: number;
  stage: string;
  closeDate: string | null;
  ownerId: number;
}

@Injectable({ providedIn: 'root' })
export class OpportunityApiService {
  private opportunitiesUrl = `${environment.apiUrl}/opportunities`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<OpportunityDto[]> {
    return this.http.get<OpportunityDto[]>(this.opportunitiesUrl);
  }

  getById(id: number): Observable<OpportunityDto> {
    return this.http.get<OpportunityDto>(`${this.opportunitiesUrl}/${id}`);
  }

  create(dto: CreateOpportunityDto): Observable<OpportunityDto> {
    return this.http.post<OpportunityDto>(this.opportunitiesUrl, dto);
  }

  update(id: number, dto: UpdateOpportunityDto): Observable<OpportunityDto> {
    return this.http.put<OpportunityDto>(`${this.opportunitiesUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.opportunitiesUrl}/${id}`);
  }
}

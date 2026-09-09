import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface AuditLogDto {
  id: number;
  userId?: number | null;
  userName?: string | null;
  action: string;
  method: string;
  path: string;
  statusCode: number;
  ipAddress?: string | null;
  durationMs: number;
  createdDate: string;
}

@Injectable({ providedIn: 'root' })
export class AuditLogApiService {
  private auditLogsUrl = `${environment.apiUrl}/audit-logs`;

  constructor(private http: HttpClient) {}

  getPaged(page = 1, pageSize = 50, search?: string | null): Observable<PagedResult<AuditLogDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    return this.http.get<PagedResult<AuditLogDto>>(this.auditLogsUrl, { params });
  }
}

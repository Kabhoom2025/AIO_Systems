import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { Table, CreateTableRequest, UpdateTableRequest } from '../models/table.model';

@Injectable({ providedIn: 'root' })
export class TableService {
  private readonly apiUrl = `${environment.apiUrl}/table`;
  private http = inject(HttpClient);

  getAll(): Observable<ApiResponse<Table[]>> {
    return this.http.get<ApiResponse<Table[]>>(this.apiUrl);
  }

  create(dto: CreateTableRequest): Observable<ApiResponse<Table>> {
    return this.http.post<ApiResponse<Table>>(this.apiUrl, dto);
  }

  update(id: number, dto: UpdateTableRequest): Observable<ApiResponse<Table>> {
    return this.http.put<ApiResponse<Table>>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.apiUrl}/${id}`);
  }
}

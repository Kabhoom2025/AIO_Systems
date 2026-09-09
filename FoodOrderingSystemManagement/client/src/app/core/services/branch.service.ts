import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { Branch, CreateBranchRequest, UpdateBranchRequest, BranchReport } from '../models/branch.model';

@Injectable({ providedIn: 'root' })
export class BranchService {
  private readonly apiUrl = `${environment.apiUrl}/branches`;
  private http = inject(HttpClient);

  getByOrganization(organizationId: number): Observable<ApiResponse<Branch[]>> {
    return this.http.get<ApiResponse<Branch[]>>(this.apiUrl, { params: { organizationId } });
  }

  create(dto: CreateBranchRequest): Observable<ApiResponse<Branch>> {
    return this.http.post<ApiResponse<Branch>>(this.apiUrl, dto);
  }

  update(id: number, dto: UpdateBranchRequest): Observable<ApiResponse<Branch>> {
    return this.http.put<ApiResponse<Branch>>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.apiUrl}/${id}`);
  }

  getReports(organizationId: number): Observable<ApiResponse<BranchReport[]>> {
    return this.http.get<ApiResponse<BranchReport[]>>(`${this.apiUrl}/reports`, { params: { organizationId } });
  }
}

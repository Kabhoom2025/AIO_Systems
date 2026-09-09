import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class RolePermissionService {
  private readonly apiUrl = `${environment.apiUrl}/permission`;
  private http = inject(HttpClient);

  getForRole(roleId: number): Observable<ApiResponse<string[]>> {
    return this.http.get<ApiResponse<string[]>>(`${this.apiUrl}/role/${roleId}`);
  }

  saveForRole(roleId: number, features: string[]): Observable<ApiResponse<unknown>> {
    return this.http.put<ApiResponse<unknown>>(`${this.apiUrl}/role/${roleId}`, features);
  }
}

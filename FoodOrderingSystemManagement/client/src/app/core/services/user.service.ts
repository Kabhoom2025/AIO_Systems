import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { AppUser, CreateUserRequest, UpdateUserRequest } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/user`;

  getAll(): Observable<ApiResponse<AppUser[]>> {
    return this.http.get<ApiResponse<AppUser[]>>(this.base);
  }

  create(payload: CreateUserRequest): Observable<ApiResponse<AppUser>> {
    return this.http.post<ApiResponse<AppUser>>(this.base, payload);
  }

  update(id: number, payload: UpdateUserRequest): Observable<ApiResponse<AppUser>> {
    return this.http.put<ApiResponse<AppUser>>(`${this.base}/${id}`, payload);
  }

  toggleActive(id: number): Observable<ApiResponse<object>> {
    return this.http.patch<ApiResponse<object>>(`${this.base}/${id}/toggle-active`, {});
  }
}

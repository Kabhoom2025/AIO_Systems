import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { PermissionsService } from './permissions.service';

export interface LoginResponse {
  token: string;
  refreshToken: string;
  userId: number;
  employeeId: number | null;
  name: string;
  email: string;
  role: string;
  organizationId: number;
  organizationName: string;
  permissions: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(private http: HttpClient, private permissions: PermissionsService) {}

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, { email, password }).pipe(
      tap(res => this.persistSession(res))
    );
  }

  changePassword(currentPassword: string, newPassword: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/auth/change-password`, { currentPassword, newPassword });
  }

  forgotPassword(email: string): Observable<{ message: string; token?: string }> {
    return this.http.post<{ message: string; token?: string }>(`${environment.apiUrl}/auth/forgot-password`, { email });
  }

  resetPassword(token: string, newPassword: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/auth/reset-password`, { token, newPassword });
  }

  logout() {
    const refreshToken = localStorage.getItem(environment.refreshTokenKey);
    if (refreshToken) {
      this.http.post(`${environment.apiUrl}/auth/logout`, { refreshToken }).subscribe({ error: () => {} });
    }
    localStorage.removeItem(environment.tokenKey);
    localStorage.removeItem(environment.refreshTokenKey);
    localStorage.removeItem(environment.userKey);
    this.permissions.reload();
  }

  private persistSession(res: LoginResponse) {
    localStorage.setItem(environment.tokenKey, res.token);
    localStorage.setItem(environment.refreshTokenKey, res.refreshToken);
    localStorage.setItem(environment.userKey, JSON.stringify({
      name: res.name, email: res.email, role: res.role, organizationName: res.organizationName
    }));
    this.permissions.reload();
  }
}

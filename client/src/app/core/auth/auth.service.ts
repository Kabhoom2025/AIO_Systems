import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { LoginRequest, LoginResponse } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _currentUser = signal<LoginResponse | null>(this.readStoredUser());
  readonly currentUser = this._currentUser.asReadonly();
  readonly userName = computed(() => this._currentUser()?.name ?? '');

  constructor(private http: HttpClient, private router: Router) {}

  private readStoredUser(): LoginResponse | null {
    const raw = localStorage.getItem(environment.userKey);
    return raw ? (JSON.parse(raw) as LoginResponse) : null;
  }

  login(request: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.http
      .post<ApiResponse<LoginResponse>>(`${environment.apiUrl}/auth/login`, request)
      .pipe(
        tap((res) => {
          if (res.success && res.data) {
            localStorage.setItem(environment.tokenKey, res.data.token);
            localStorage.setItem(environment.userKey, JSON.stringify(res.data));
            this._currentUser.set(res.data);
          }
        })
      );
  }

  logout(): void {
    localStorage.removeItem(environment.tokenKey);
    localStorage.removeItem(environment.userKey);
    this._currentUser.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(environment.tokenKey);
  }

  isAuthenticated(): boolean {
    return !!this.getToken() && !!this._currentUser();
  }

  isSuperAdmin(): boolean {
    return this._currentUser()?.isSuperAdmin === true;
  }
}

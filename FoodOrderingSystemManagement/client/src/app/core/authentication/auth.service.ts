import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { APP_CONSTANTS } from '../constants/app.constants';
import { ApiResponse } from '../models/api-response.model';
import { LoginRequest, LoginResponse } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly directApiUrl = `${environment.apiUrlDirect}/auth`;

  private _currentUser = signal<LoginResponse | null>(this.loadUser());
  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated  = computed(() => !!this._currentUser());
  readonly userRole         = computed(() => this._currentUser()?.roleName ?? '');
  readonly userName         = computed(() => this._currentUser()?.name ?? '');
  readonly enabledModules   = computed(() => this._currentUser()?.enabledModules ?? []);
  readonly isSuperAdmin     = computed(() => this._currentUser()?.isSuperAdmin ?? false);

  constructor(private http: HttpClient, private router: Router) {}

  login(request: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/login`, request).pipe(
      // Gateway (port 5000) isn't running — status 0 means the request never reached a server.
      // Fall back to hitting the FoodOrder API directly so login also works standalone.
      catchError((err) => err.status === 0
        ? this.http.post<ApiResponse<LoginResponse>>(`${this.directApiUrl}/login`, request)
        : throwError(() => err)),
      tap((res) => {
        if (res.success) {
          localStorage.setItem(APP_CONSTANTS.TOKEN_KEY, res.data.token);
          localStorage.setItem(APP_CONSTANTS.USER_KEY, JSON.stringify(res.data));
          localStorage.setItem('organizationId', String(res.data.organizationId ?? null));
          localStorage.setItem('organizationName', res.data.organizationName ?? '');
          localStorage.setItem('isSuperAdmin', String(res.data.isSuperAdmin ?? false));
          this._currentUser.set(res.data);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem(APP_CONSTANTS.TOKEN_KEY);
    localStorage.removeItem(APP_CONSTANTS.USER_KEY);
    localStorage.removeItem('organizationId');
    localStorage.removeItem('organizationName');
    localStorage.removeItem('isSuperAdmin');
    this._currentUser.set(null);
    this.router.navigate(['/auth/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(APP_CONSTANTS.TOKEN_KEY);
  }

  private loadUser(): LoginResponse | null {
    try {
      const raw = localStorage.getItem(APP_CONSTANTS.USER_KEY);
      return raw ? (JSON.parse(raw) as LoginResponse) : null;
    } catch {
      return null;
    }
  }

  reloadUser(): void {
    this._currentUser.set(this.loadUser());
  }

  getOrganizationId(): number | null {
    const id = localStorage.getItem('organizationId');
    return id && id !== 'null' ? parseInt(id) : null;
  }

  getOrganizationName(): string {
    return localStorage.getItem('organizationName') ?? '';
  }

  isSuperAdminUser(): boolean {
    return localStorage.getItem('isSuperAdmin') === 'true';
  }
}

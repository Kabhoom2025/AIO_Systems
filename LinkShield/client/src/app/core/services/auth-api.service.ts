import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth.models';
import { TokenStorageService } from './token-storage.service';

export interface AuthenticatedUser {
  userId: string;
  email: string;
  roles: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly tokenStorage = inject(TokenStorageService);
  private readonly baseUrl = environment.apiBaseUrl;

  readonly currentUser = signal<AuthenticatedUser | null>(this.decodeStoredToken());

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/v1/auth/register`, request)
      .pipe(tap((response) => this.onAuthenticated(response)));
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/v1/auth/login`, request)
      .pipe(tap((response) => this.onAuthenticated(response)));
  }

  logout(): void {
    this.tokenStorage.clear();
    this.currentUser.set(null);
  }

  hasAnyRole(...roles: string[]): boolean {
    const userRoles = this.currentUser()?.roles ?? [];
    return roles.some((role) => userRoles.includes(role));
  }

  private onAuthenticated(response: AuthResponse): void {
    this.tokenStorage.setAccessToken(response.accessToken);
    this.currentUser.set({ userId: response.userId, email: response.email, roles: response.roles });
  }

  /** Restores session state from a previously stored JWT so a page refresh doesn't log the
   * user out client-side (the token itself is still validated server-side on every request —
   * this only rehydrates UI state, it grants no access on its own). */
  private decodeStoredToken(): AuthenticatedUser | null {
    const token = this.tokenStorage.getAccessToken();
    if (!token) return null;

    try {
      const payloadSegment = token.split('.')[1];
      const base64 = payloadSegment.replace(/-/g, '+').replace(/_/g, '/');
      const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=');
      const payload = JSON.parse(atob(padded));

      const roleClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      const roles = Array.isArray(roleClaim) ? roleClaim : roleClaim ? [roleClaim] : [];
      const email = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ?? payload.email;
      const userId = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ?? payload.sub;

      if (payload.exp && Date.now() / 1000 > payload.exp) {
        this.tokenStorage.clear();
        return null;
      }

      return { userId, email, roles };
    } catch {
      return null;
    }
  }
}

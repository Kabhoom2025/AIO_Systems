import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

export interface LoginResponse {
  token: string;
  userId: number;
  name: string;
  email: string;
  role: string;
  organizationId: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, { email, password }).pipe(
      tap(res => this.persistSession(res))
    );
  }

  logout() {
    localStorage.removeItem(environment.tokenKey);
    localStorage.removeItem(environment.userKey);
  }

  private persistSession(res: LoginResponse) {
    localStorage.setItem(environment.tokenKey, res.token);
    localStorage.setItem(environment.userKey, JSON.stringify({ name: res.name, email: res.email, role: res.role }));
  }
}

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export const PATIENT_TOKEN_KEY = 'patient_token';
export const PATIENT_USER_KEY = 'patient_user';

export interface RequestOtpResponse {
  message: string;
  devOtp?: string;
}

export interface PatientLoginResponse {
  token: string;
  patientId: number;
  name: string;
  phone: string;
  organizationId: number;
  organizationName: string;
}

@Injectable({ providedIn: 'root' })
export class PatientAuthService {
  constructor(private http: HttpClient) {}

  requestOtp(phone: string) {
    return this.http.post<RequestOtpResponse>(`${environment.apiUrl}/patient-auth/request-otp`, { phone });
  }

  verifyOtp(phone: string, code: string) {
    return this.http.post<PatientLoginResponse>(`${environment.apiUrl}/patient-auth/verify-otp`, { phone, code });
  }

  saveSession(res: PatientLoginResponse) {
    localStorage.setItem(PATIENT_TOKEN_KEY, res.token);
    localStorage.setItem(PATIENT_USER_KEY, JSON.stringify(res));
  }

  getUser(): PatientLoginResponse | null {
    const raw = localStorage.getItem(PATIENT_USER_KEY);
    if (!raw) return null;
    try { return JSON.parse(raw); } catch { return null; }
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem(PATIENT_TOKEN_KEY);
  }

  getAuthHeaders(): Record<string, string> {
    const token = localStorage.getItem(PATIENT_TOKEN_KEY);
    return token ? { Authorization: `Bearer ${token}` } : {};
  }

  logout() {
    localStorage.removeItem(PATIENT_TOKEN_KEY);
    localStorage.removeItem(PATIENT_USER_KEY);
  }
}

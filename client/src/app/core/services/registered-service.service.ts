import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateRegisteredServiceRequest,
  RegisteredService,
  RunMode,
  UpdateRegisteredServiceRequest,
} from '../models/registered-service.model';

@Injectable({ providedIn: 'root' })
export class RegisteredServiceService {
  private readonly base = `${environment.apiUrl}/registered-services`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<RegisteredService[]> {
    return this.http.get<RegisteredService[]>(this.base);
  }

  getById(id: number): Observable<RegisteredService> {
    return this.http.get<RegisteredService>(`${this.base}/${id}`);
  }

  create(request: CreateRegisteredServiceRequest): Observable<RegisteredService> {
    return this.http.post<RegisteredService>(this.base, request);
  }

  update(id: number, request: UpdateRegisteredServiceRequest): Observable<RegisteredService> {
    return this.http.put<RegisteredService>(`${this.base}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  start(id: number, mode: RunMode = 'Native'): Observable<RegisteredService> {
    return this.http.post<RegisteredService>(`${this.base}/${id}/start?mode=${mode}`, {});
  }

  stop(id: number, mode: RunMode = 'Native'): Observable<RegisteredService> {
    return this.http.post<RegisteredService>(`${this.base}/${id}/stop?mode=${mode}`, {});
  }

  /**
   * Pings a service through the gateway using its configured route prefix + health path.
   * Gateway routes like "/api/restaurant/{**catch-all}" strip the "/api" segment before
   * forwarding, so a stored HealthCheckPath of "/api/system-health" becomes
   * "{apiUrl}/restaurant/system-health" here.
   */
  checkHealth(service: RegisteredService): Observable<unknown> {
    const prefix = service.routePrefix.replace(/^\/|\/$/g, '');
    const path = (service.healthCheckPath ?? '').replace(/^\/?(api\/)?/, '');
    return this.http.get(`${environment.apiUrl}/${prefix}/${path}`);
  }
}

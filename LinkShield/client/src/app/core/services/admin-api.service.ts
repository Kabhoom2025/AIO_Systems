import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  ApiClient, AuditLog, BrandProfile, CreateApiClientRequest, CreateApiKeyRequest, CreateBrandProfileRequest,
  CreateRiskRuleRequest, CreatedApiKey, RiskRule, UpdateRiskRuleRequest
} from '../models/admin.models';
import { PagedResult } from '../models/scan.models';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  getBrands(): Observable<BrandProfile[]> {
    return this.http.get<BrandProfile[]>(`${this.baseUrl}/v1/brands`);
  }

  createBrand(request: CreateBrandProfileRequest): Observable<BrandProfile> {
    return this.http.post<BrandProfile>(`${this.baseUrl}/v1/brands`, request);
  }

  getRiskRules(): Observable<RiskRule[]> {
    return this.http.get<RiskRule[]>(`${this.baseUrl}/v1/risk-rules`);
  }

  createRiskRule(request: CreateRiskRuleRequest): Observable<RiskRule> {
    return this.http.post<RiskRule>(`${this.baseUrl}/v1/risk-rules`, request);
  }

  updateRiskRule(id: string, request: UpdateRiskRuleRequest): Observable<RiskRule> {
    return this.http.put<RiskRule>(`${this.baseUrl}/v1/risk-rules/${id}`, request);
  }

  getAuditLogs(page: number, pageSize: number): Observable<PagedResult<AuditLog>> {
    return this.http.get<PagedResult<AuditLog>>(`${this.baseUrl}/v1/audit-logs`, { params: { page, pageSize } });
  }

  getApiClients(): Observable<ApiClient[]> {
    return this.http.get<ApiClient[]>(`${this.baseUrl}/v1/api-clients`);
  }

  createApiClient(request: CreateApiClientRequest): Observable<ApiClient> {
    return this.http.post<ApiClient>(`${this.baseUrl}/v1/api-clients`, request);
  }

  createApiKey(clientId: string, request: CreateApiKeyRequest): Observable<CreatedApiKey> {
    return this.http.post<CreatedApiKey>(`${this.baseUrl}/v1/api-clients/${clientId}/keys`, request);
  }

  revokeApiKey(keyId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/v1/api-clients/keys/${keyId}`);
  }
}

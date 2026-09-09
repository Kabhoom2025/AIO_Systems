import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { DashboardSummary, DashboardTrends } from '../models/dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  getSummary(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(`${this.baseUrl}/v1/dashboard/summary`);
  }

  getTrends(days: number): Observable<DashboardTrends> {
    return this.http.get<DashboardTrends>(`${this.baseUrl}/v1/dashboard/trends`, { params: { days } });
  }
}

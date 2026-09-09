import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { PagedResult, ScanDetail, ScanSummary, UrlAnalysisResult } from '../models/scan.models';

@Injectable({ providedIn: 'root' })
export class ScanApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  submitScan(url: string): Observable<ScanDetail> {
    return this.http.post<ScanDetail>(`${this.baseUrl}/v1/scans`, { url });
  }

  getScan(scanId: string): Observable<ScanDetail> {
    return this.http.get<ScanDetail>(`${this.baseUrl}/v1/scans/${scanId}`);
  }

  getScans(page: number, pageSize: number): Observable<PagedResult<ScanSummary>> {
    return this.http.get<PagedResult<ScanSummary>>(`${this.baseUrl}/v1/scans`, {
      params: { page, pageSize }
    });
  }

  deleteScan(scanId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/v1/scans/${scanId}`);
  }

  analyzeUrl(url: string): Observable<UrlAnalysisResult> {
    return this.http.post<UrlAnalysisResult>(`${this.baseUrl}/v1/url/analyze`, { url });
  }
}

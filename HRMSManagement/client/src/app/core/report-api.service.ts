import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface HeadcountReportRow {
  departmentName: string;
  branchName: string;
  activeCount: number;
  onNoticeCount: number;
  exitedCount: number;
}

export interface AttendanceMonthlyReportRow {
  employeeCode: string;
  employeeName: string;
  presentDays: number;
  leaveDays: number;
  lateCount: number;
  overtimeHours: number;
}

export interface LeaveReportRow {
  employeeCode: string;
  employeeName: string;
  leaveTypeName: string;
  allocated: number;
  used: number;
  available: number;
}

export interface PayrollSummaryReportRow {
  year: number;
  month: number;
  status: string;
  employeeCount: number;
  totalGross: number;
  totalDeductions: number;
  totalNet: number;
}

export interface RecruitmentPipelineReportRow {
  departmentName: string;
  title: string;
  status: string;
  vacancies: number;
  totalCandidates: number;
  applied: number;
  screening: number;
  interview: number;
  offered: number;
  hired: number;
  rejected: number;
}

export interface AssetReportRow {
  assetTag: string;
  name: string;
  category: string;
  status: string;
  condition: string;
  currentHolder: string;
  purchaseCost?: number | null;
  warrantyUntil?: string | null;
}

@Injectable({ providedIn: 'root' })
export class ReportApiService {
  private baseUrl = `${environment.apiUrl}/reports`;

  constructor(private http: HttpClient) {}

  getHeadcount(): Observable<HeadcountReportRow[]> {
    return this.http.get<HeadcountReportRow[]>(`${this.baseUrl}/headcount`);
  }

  downloadHeadcount(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/headcount`, {
      params: new HttpParams().set('format', 'csv'),
      responseType: 'blob'
    });
  }

  getAttendanceMonthly(year: number, month: number): Observable<AttendanceMonthlyReportRow[]> {
    const params = new HttpParams().set('year', year).set('month', month);
    return this.http.get<AttendanceMonthlyReportRow[]>(`${this.baseUrl}/attendance-monthly`, { params });
  }

  downloadAttendanceMonthly(year: number, month: number): Observable<Blob> {
    const params = new HttpParams().set('year', year).set('month', month).set('format', 'csv');
    return this.http.get(`${this.baseUrl}/attendance-monthly`, { params, responseType: 'blob' });
  }

  getLeave(year: number): Observable<LeaveReportRow[]> {
    const params = new HttpParams().set('year', year);
    return this.http.get<LeaveReportRow[]>(`${this.baseUrl}/leave`, { params });
  }

  downloadLeave(year: number): Observable<Blob> {
    const params = new HttpParams().set('year', year).set('format', 'csv');
    return this.http.get(`${this.baseUrl}/leave`, { params, responseType: 'blob' });
  }

  getPayrollSummary(year: number): Observable<PayrollSummaryReportRow[]> {
    const params = new HttpParams().set('year', year);
    return this.http.get<PayrollSummaryReportRow[]>(`${this.baseUrl}/payroll-summary`, { params });
  }

  downloadPayrollSummary(year: number): Observable<Blob> {
    const params = new HttpParams().set('year', year).set('format', 'csv');
    return this.http.get(`${this.baseUrl}/payroll-summary`, { params, responseType: 'blob' });
  }

  getRecruitmentPipeline(): Observable<RecruitmentPipelineReportRow[]> {
    return this.http.get<RecruitmentPipelineReportRow[]>(`${this.baseUrl}/recruitment-pipeline`);
  }

  downloadRecruitmentPipeline(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/recruitment-pipeline`, {
      params: new HttpParams().set('format', 'csv'),
      responseType: 'blob'
    });
  }

  getAssets(): Observable<AssetReportRow[]> {
    return this.http.get<AssetReportRow[]>(`${this.baseUrl}/assets`);
  }

  downloadAssets(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/assets`, {
      params: new HttpParams().set('format', 'csv'),
      responseType: 'blob'
    });
  }
}

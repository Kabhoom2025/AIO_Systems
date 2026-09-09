import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AttendanceRecordDto {
  id: number;
  employeeId: number;
  date: string;
  checkInAt: string | null;
  checkOutAt: string | null;
  status: string;
  workedMinutes: number;
  overtimeMinutes: number;
  lateMinutes: number;
  earlyExitMinutes: number;
  source: string;
  location: string | null;
  notes: string | null;
}

export interface AttendanceDayDto {
  employeeId: number;
  employeeCode: string;
  employeeName: string;
  departmentName: string;
  checkInAt: string | null;
  checkOutAt: string | null;
  status: string;
  workedMinutes: number;
  lateMinutes: number;
  overtimeMinutes: number;
}

export interface AttendanceSummaryDto {
  totalEmployees: number;
  present: number;
  absent: number;
  onLeave: number;
  late: number;
}

export interface CheckInDto {
  location?: string | null;
  source?: string;
}

export interface RegularizationDto {
  id: number;
  employeeId: number;
  employeeName: string;
  date: string;
  requestedCheckIn: string | null;
  requestedCheckOut: string | null;
  reason: string;
  status: string;
  reviewedByUserId: number | null;
  reviewedAt: string | null;
  reviewNotes: string | null;
  createdDate: string;
}

export interface CreateRegularizationDto {
  date: string;
  requestedCheckIn?: string | null;
  requestedCheckOut?: string | null;
  reason: string;
}

export interface ReviewDto {
  notes?: string | null;
}

@Injectable({ providedIn: 'root' })
export class AttendanceApiService {
  private base = `${environment.apiUrl}/attendance`;

  constructor(private http: HttpClient) {}

  checkIn(dto: CheckInDto): Observable<AttendanceRecordDto> {
    return this.http.post<AttendanceRecordDto>(`${this.base}/check-in`, dto);
  }

  checkOut(): Observable<AttendanceRecordDto> {
    return this.http.post<AttendanceRecordDto>(`${this.base}/check-out`, {});
  }

  today(): Observable<AttendanceRecordDto> {
    return this.http.get<AttendanceRecordDto>(`${this.base}/today`);
  }

  daily(date?: string): Observable<AttendanceDayDto[]> {
    const params: Record<string, string> = {};
    if (date) params['date'] = date;
    return this.http.get<AttendanceDayDto[]>(`${this.base}/daily`, { params });
  }

  employeeMonth(employeeId: number, year: number, month: number): Observable<AttendanceRecordDto[]> {
    return this.http.get<AttendanceRecordDto[]>(`${this.base}/employee/${employeeId}`, {
      params: { year, month }
    });
  }

  my(year: number, month: number): Observable<AttendanceRecordDto[]> {
    return this.http.get<AttendanceRecordDto[]>(`${this.base}/my`, { params: { year, month } });
  }

  summary(date?: string): Observable<AttendanceSummaryDto> {
    const params: Record<string, string> = {};
    if (date) params['date'] = date;
    return this.http.get<AttendanceSummaryDto>(`${this.base}/summary`, { params });
  }

  createRegularization(dto: CreateRegularizationDto): Observable<RegularizationDto> {
    return this.http.post<RegularizationDto>(`${this.base}/regularizations`, dto);
  }

  regularizations(): Observable<RegularizationDto[]> {
    return this.http.get<RegularizationDto[]>(`${this.base}/regularizations`);
  }

  myRegularizations(): Observable<RegularizationDto[]> {
    return this.http.get<RegularizationDto[]>(`${this.base}/regularizations/my`);
  }

  approveRegularization(id: number, dto: ReviewDto): Observable<RegularizationDto> {
    return this.http.post<RegularizationDto>(`${this.base}/regularizations/${id}/approve`, dto);
  }

  rejectRegularization(id: number, dto: ReviewDto): Observable<RegularizationDto> {
    return this.http.post<RegularizationDto>(`${this.base}/regularizations/${id}/reject`, dto);
  }
}

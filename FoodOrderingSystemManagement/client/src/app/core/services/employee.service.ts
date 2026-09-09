import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response.model';
import { AppUser } from '../models/user.model';
import {
  AttendanceRecord, UpsertAttendanceRequest,
  EmployeeSalaryConfig, UpdateSalaryConfigRequest,
  SalaryPayment, CreateSalaryPaymentRequest,
  ShiftDefinition, CreateShiftDefinitionRequest,
  ShiftAssignment, CreateShiftAssignmentRequest,
  PerformanceReview, CreatePerformanceReviewRequest,
  UpdateUserRequest,
} from '../models/employee.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  private readonly http   = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/employee`;

  // ── User edit / delete ──────────────────────────────────────────────────

  updateUser(id: number, dto: UpdateUserRequest): Observable<ApiResponse<AppUser>> {
    return this.http.put<ApiResponse<AppUser>>(`${this.apiUrl}/${id}`, dto);
  }

  deleteUser(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.apiUrl}/${id}`);
  }

  // ── Attendance ──────────────────────────────────────────────────────────

  getAttendanceByDate(date: string): Observable<ApiResponse<AttendanceRecord[]>> {
    return this.http.get<ApiResponse<AttendanceRecord[]>>(
      `${this.apiUrl}/attendance?date=${date}`
    );
  }

  getAttendanceByUser(userId: number, month: number, year: number): Observable<ApiResponse<AttendanceRecord[]>> {
    return this.http.get<ApiResponse<AttendanceRecord[]>>(
      `${this.apiUrl}/attendance/user/${userId}?month=${month}&year=${year}`
    );
  }

  upsertAttendance(req: UpsertAttendanceRequest): Observable<ApiResponse<AttendanceRecord>> {
    return this.http.post<ApiResponse<AttendanceRecord>>(`${this.apiUrl}/attendance`, req);
  }

  // ── Salary ──────────────────────────────────────────────────────────────

  getSalaryConfigs(): Observable<ApiResponse<EmployeeSalaryConfig[]>> {
    return this.http.get<ApiResponse<EmployeeSalaryConfig[]>>(`${this.apiUrl}/salary/configs`);
  }

  updateSalaryConfig(userId: number, req: UpdateSalaryConfigRequest): Observable<ApiResponse<EmployeeSalaryConfig>> {
    return this.http.put<ApiResponse<EmployeeSalaryConfig>>(`${this.apiUrl}/salary/config/${userId}`, req);
  }

  getPayments(userId?: number): Observable<ApiResponse<SalaryPayment[]>> {
    const url = userId ? `${this.apiUrl}/salary/payments?userId=${userId}` : `${this.apiUrl}/salary/payments`;
    return this.http.get<ApiResponse<SalaryPayment[]>>(url);
  }

  createPayment(req: CreateSalaryPaymentRequest): Observable<ApiResponse<SalaryPayment>> {
    return this.http.post<ApiResponse<SalaryPayment>>(`${this.apiUrl}/salary/payments`, req);
  }

  markPaid(id: number): Observable<ApiResponse<SalaryPayment>> {
    return this.http.patch<ApiResponse<SalaryPayment>>(`${this.apiUrl}/salary/payments/${id}/mark-paid`, {});
  }

  // ── Shifts ──────────────────────────────────────────────────────────────

  getShiftDefinitions(): Observable<ApiResponse<ShiftDefinition[]>> {
    return this.http.get<ApiResponse<ShiftDefinition[]>>(`${this.apiUrl}/shifts`);
  }

  createShiftDefinition(req: CreateShiftDefinitionRequest): Observable<ApiResponse<ShiftDefinition>> {
    return this.http.post<ApiResponse<ShiftDefinition>>(`${this.apiUrl}/shifts`, req);
  }

  deleteShiftDefinition(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.apiUrl}/shifts/${id}`);
  }

  getShiftAssignments(startDate: string, endDate: string): Observable<ApiResponse<ShiftAssignment[]>> {
    return this.http.get<ApiResponse<ShiftAssignment[]>>(
      `${this.apiUrl}/shifts/assignments?startDate=${startDate}&endDate=${endDate}`
    );
  }

  createShiftAssignment(req: CreateShiftAssignmentRequest): Observable<ApiResponse<ShiftAssignment>> {
    return this.http.post<ApiResponse<ShiftAssignment>>(`${this.apiUrl}/shifts/assignments`, req);
  }

  deleteShiftAssignment(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.apiUrl}/shifts/assignments/${id}`);
  }

  // ── Performance ─────────────────────────────────────────────────────────

  getReviews(userId?: number): Observable<ApiResponse<PerformanceReview[]>> {
    const url = userId ? `${this.apiUrl}/performance?userId=${userId}` : `${this.apiUrl}/performance`;
    return this.http.get<ApiResponse<PerformanceReview[]>>(url);
  }

  createReview(req: CreatePerformanceReviewRequest): Observable<ApiResponse<PerformanceReview>> {
    return this.http.post<ApiResponse<PerformanceReview>>(`${this.apiUrl}/performance`, req);
  }

  deleteReview(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.apiUrl}/performance/${id}`);
  }
}

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { EmployeeDto } from './employee-api.service';
import { LeaveRequestDto } from './leave-request-api.service';
import { LeaveTypeDto } from './leave-type-api.service';

export interface CreateMyLeaveRequestDto {
  leaveTypeId: number;
  startDate: string;
  endDate: string;
  daysRequested: number;
  reason: string | null;
}

export interface MyPayslipDto {
  payRunId: number;
  runNumber: string;
  periodMonth: number;
  periodYear: number;

  basicSalary: number;
  hra: number;
  otherAllowances: number;
  deductions: number;
  grossPay: number;
  netPay: number;
}

@Injectable({ providedIn: 'root' })
export class EmployeePortalApiService {
  private myUrl = `${environment.apiUrl}/my`;

  constructor(private http: HttpClient) {}

  getMyProfile(): Observable<EmployeeDto> {
    return this.http.get<EmployeeDto>(`${this.myUrl}/profile`);
  }

  getMyLeaveRequests(): Observable<LeaveRequestDto[]> {
    return this.http.get<LeaveRequestDto[]>(`${this.myUrl}/leave-requests`);
  }

  createMyLeaveRequest(dto: CreateMyLeaveRequestDto): Observable<LeaveRequestDto> {
    return this.http.post<LeaveRequestDto>(`${this.myUrl}/leave-requests`, dto);
  }

  cancelMyLeaveRequest(id: number): Observable<LeaveRequestDto> {
    return this.http.post<LeaveRequestDto>(`${this.myUrl}/leave-requests/${id}/cancel`, {});
  }

  getMyPayslips(): Observable<MyPayslipDto[]> {
    return this.http.get<MyPayslipDto[]>(`${this.myUrl}/payslips`);
  }

  getLeaveTypes(): Observable<LeaveTypeDto[]> {
    return this.http.get<LeaveTypeDto[]>(`${this.myUrl}/leave-types`);
  }
}

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface LeaveTypeDto {
  id: number;
  name: string;
  code: string;
  isPaid: boolean;
  annualQuota: number;
  maxCarryForward: number;
  requiresApproval: boolean;
  color: string | null;
  isActive: boolean;
}

export interface CreateLeaveTypeDto {
  name: string;
  code: string;
  isPaid: boolean;
  annualQuota: number;
  maxCarryForward: number;
  requiresApproval: boolean;
  color?: string | null;
}

export interface UpdateLeaveTypeDto extends CreateLeaveTypeDto {
  isActive: boolean;
}

export interface LeaveBalanceDto {
  id: number;
  employeeId: number;
  leaveTypeId: number;
  leaveTypeName: string;
  year: number;
  allocated: number;
  used: number;
  carriedForward: number;
  available: number;
}

export interface LeaveRequestDto {
  id: number;
  employeeId: number;
  employeeName: string;
  leaveTypeId: number;
  leaveTypeName: string;
  startDate: string;
  endDate: string;
  days: number;
  isHalfDay: boolean;
  reason: string;
  status: string;
  reviewedByUserId: number | null;
  reviewedAt: string | null;
  reviewNotes: string | null;
  createdDate: string;
}

export interface CreateLeaveRequestDto {
  leaveTypeId: number;
  startDate: string;
  endDate: string;
  isHalfDay: boolean;
  reason: string;
}

export interface LeaveCalendarDto {
  employeeName: string;
  leaveTypeName: string;
  color: string | null;
  startDate: string;
  endDate: string;
}

export interface AllocateBalancesDto {
  year: number;
}

export interface ReviewDto {
  notes?: string | null;
}

@Injectable({ providedIn: 'root' })
export class LeaveApiService {
  private base = `${environment.apiUrl}/leaves`;

  constructor(private http: HttpClient) {}

  // Leave types
  getTypes(): Observable<LeaveTypeDto[]> {
    return this.http.get<LeaveTypeDto[]>(`${this.base}/types`);
  }

  createType(dto: CreateLeaveTypeDto): Observable<LeaveTypeDto> {
    return this.http.post<LeaveTypeDto>(`${this.base}/types`, dto);
  }

  updateType(id: number, dto: UpdateLeaveTypeDto): Observable<LeaveTypeDto> {
    return this.http.put<LeaveTypeDto>(`${this.base}/types/${id}`, dto);
  }

  deleteType(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/types/${id}`);
  }

  // Balances
  myBalances(): Observable<LeaveBalanceDto[]> {
    return this.http.get<LeaveBalanceDto[]>(`${this.base}/balances/my`);
  }

  balancesForEmployee(employeeId: number): Observable<LeaveBalanceDto[]> {
    return this.http.get<LeaveBalanceDto[]>(`${this.base}/balances/employee/${employeeId}`);
  }

  allocateBalances(dto: AllocateBalancesDto): Observable<LeaveBalanceDto[]> {
    return this.http.post<LeaveBalanceDto[]>(`${this.base}/balances/allocate`, dto);
  }

  // Requests
  createRequest(dto: CreateLeaveRequestDto): Observable<LeaveRequestDto> {
    return this.http.post<LeaveRequestDto>(`${this.base}`, dto);
  }

  myRequests(): Observable<LeaveRequestDto[]> {
    return this.http.get<LeaveRequestDto[]>(`${this.base}/my`);
  }

  requests(status?: string): Observable<LeaveRequestDto[]> {
    const params: Record<string, string> = {};
    if (status) params['status'] = status;
    return this.http.get<LeaveRequestDto[]>(`${this.base}`, { params });
  }

  pendingRequests(): Observable<LeaveRequestDto[]> {
    return this.http.get<LeaveRequestDto[]>(`${this.base}/pending`);
  }

  approveRequest(id: number, dto: ReviewDto): Observable<LeaveRequestDto> {
    return this.http.post<LeaveRequestDto>(`${this.base}/${id}/approve`, dto);
  }

  rejectRequest(id: number, dto: ReviewDto): Observable<LeaveRequestDto> {
    return this.http.post<LeaveRequestDto>(`${this.base}/${id}/reject`, dto);
  }

  cancelRequest(id: number): Observable<LeaveRequestDto> {
    return this.http.post<LeaveRequestDto>(`${this.base}/${id}/cancel`, {});
  }

  // Calendar
  calendar(from: string, to: string): Observable<LeaveCalendarDto[]> {
    return this.http.get<LeaveCalendarDto[]>(`${this.base}/calendar`, { params: { from, to } });
  }
}

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface LeaveRequestDto {
  id: number;
  employeeId: number;
  employeeName: string;
  leaveTypeId: number;
  leaveTypeName: string;

  startDate: string;
  endDate: string;
  daysRequested: number;
  reason: string | null;
  status: string;
}

export interface CreateLeaveRequestDto {
  employeeId: number;
  leaveTypeId: number;

  startDate: string;
  endDate: string;
  daysRequested: number;
  reason: string | null;
}

export interface UpdateLeaveRequestDto {
  leaveTypeId: number;

  startDate: string;
  endDate: string;
  daysRequested: number;
  reason: string | null;
}

@Injectable({ providedIn: 'root' })
export class LeaveRequestApiService {
  private leaveRequestsUrl = `${environment.apiUrl}/leave-requests`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<LeaveRequestDto[]> {
    return this.http.get<LeaveRequestDto[]>(this.leaveRequestsUrl);
  }

  getById(id: number): Observable<LeaveRequestDto> {
    return this.http.get<LeaveRequestDto>(`${this.leaveRequestsUrl}/${id}`);
  }

  create(dto: CreateLeaveRequestDto): Observable<LeaveRequestDto> {
    return this.http.post<LeaveRequestDto>(this.leaveRequestsUrl, dto);
  }

  update(id: number, dto: UpdateLeaveRequestDto): Observable<LeaveRequestDto> {
    return this.http.put<LeaveRequestDto>(`${this.leaveRequestsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.leaveRequestsUrl}/${id}`);
  }

  approve(id: number): Observable<LeaveRequestDto> {
    return this.http.post<LeaveRequestDto>(`${this.leaveRequestsUrl}/${id}/approve`, {});
  }

  reject(id: number): Observable<LeaveRequestDto> {
    return this.http.post<LeaveRequestDto>(`${this.leaveRequestsUrl}/${id}/reject`, {});
  }

  cancel(id: number): Observable<LeaveRequestDto> {
    return this.http.post<LeaveRequestDto>(`${this.leaveRequestsUrl}/${id}/cancel`, {});
  }
}

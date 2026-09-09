import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface LeaveTypeDto {
  id: number;
  name: string;
  code: string;
  defaultDaysPerYear: number | null;
  isActive: boolean;
}

export interface CreateLeaveTypeDto {
  name: string;
  code: string;
  defaultDaysPerYear: number | null;
  isActive: boolean;
}

export interface UpdateLeaveTypeDto {
  name: string;
  defaultDaysPerYear: number | null;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class LeaveTypeApiService {
  private leaveTypesUrl = `${environment.apiUrl}/leave-types`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<LeaveTypeDto[]> {
    return this.http.get<LeaveTypeDto[]>(this.leaveTypesUrl);
  }

  getById(id: number): Observable<LeaveTypeDto> {
    return this.http.get<LeaveTypeDto>(`${this.leaveTypesUrl}/${id}`);
  }

  create(dto: CreateLeaveTypeDto): Observable<LeaveTypeDto> {
    return this.http.post<LeaveTypeDto>(this.leaveTypesUrl, dto);
  }

  update(id: number, dto: UpdateLeaveTypeDto): Observable<LeaveTypeDto> {
    return this.http.put<LeaveTypeDto>(`${this.leaveTypesUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.leaveTypesUrl}/${id}`);
  }
}

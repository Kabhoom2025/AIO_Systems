import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface EmployeeCompensationDto {
  id: number;
  employeeId: number;
  employeeName: string;
  employeeCode: string;

  basicSalary: number;
  hra: number;
  otherAllowances: number;
  deductions: number;

  grossPay: number;
  netPay: number;
}

export interface CreateEmployeeCompensationDto {
  employeeId: number;
  basicSalary: number;
  hra: number;
  otherAllowances: number;
  deductions: number;
}

export interface UpdateEmployeeCompensationDto {
  basicSalary: number;
  hra: number;
  otherAllowances: number;
  deductions: number;
}

@Injectable({ providedIn: 'root' })
export class EmployeeCompensationApiService {
  private compensationUrl = `${environment.apiUrl}/employee-compensation`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<EmployeeCompensationDto[]> {
    return this.http.get<EmployeeCompensationDto[]>(this.compensationUrl);
  }

  getById(id: number): Observable<EmployeeCompensationDto> {
    return this.http.get<EmployeeCompensationDto>(`${this.compensationUrl}/${id}`);
  }

  create(dto: CreateEmployeeCompensationDto): Observable<EmployeeCompensationDto> {
    return this.http.post<EmployeeCompensationDto>(this.compensationUrl, dto);
  }

  update(id: number, dto: UpdateEmployeeCompensationDto): Observable<EmployeeCompensationDto> {
    return this.http.put<EmployeeCompensationDto>(`${this.compensationUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.compensationUrl}/${id}`);
  }
}

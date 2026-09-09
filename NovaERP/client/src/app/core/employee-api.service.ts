import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface EmployeeDto {
  id: number;
  employeeCode: string;
  departmentId: number;
  departmentName: string;
  userId: number | null;
  userName: string | null;
  reportingManagerId: number | null;
  reportingManagerName: string | null;

  firstName: string;
  lastName: string;
  email: string;
  phone: string | null;
  dateOfBirth: string | null;
  gender: string | null;

  jobTitle: string;
  employmentType: string;
  dateOfJoining: string;
  terminationDate: string | null;
  status: string;
}

export interface CreateEmployeeDto {
  departmentId: number;
  userId: number | null;
  reportingManagerId: number | null;

  firstName: string;
  lastName: string;
  email: string;
  phone: string | null;
  dateOfBirth: string | null;
  gender: string | null;

  jobTitle: string;
  employmentType: string;
  dateOfJoining: string;
}

export interface UpdateEmployeeDto {
  userId: number | null;
  reportingManagerId: number | null;

  firstName: string;
  lastName: string;
  email: string;
  phone: string | null;
  dateOfBirth: string | null;
  gender: string | null;

  jobTitle: string;
  employmentType: string;
  dateOfJoining: string;
}

@Injectable({ providedIn: 'root' })
export class EmployeeApiService {
  private employeesUrl = `${environment.apiUrl}/employees`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<EmployeeDto[]> {
    return this.http.get<EmployeeDto[]>(this.employeesUrl);
  }

  getById(id: number): Observable<EmployeeDto> {
    return this.http.get<EmployeeDto>(`${this.employeesUrl}/${id}`);
  }

  create(dto: CreateEmployeeDto): Observable<EmployeeDto> {
    return this.http.post<EmployeeDto>(this.employeesUrl, dto);
  }

  update(id: number, dto: UpdateEmployeeDto): Observable<EmployeeDto> {
    return this.http.put<EmployeeDto>(`${this.employeesUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.employeesUrl}/${id}`);
  }

  terminate(id: number): Observable<EmployeeDto> {
    return this.http.post<EmployeeDto>(`${this.employeesUrl}/${id}/terminate`, {});
  }
}

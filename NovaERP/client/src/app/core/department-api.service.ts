import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface DepartmentDto {
  id: number;
  branchId?: number | null;
  branchName?: string | null;
  parentId?: number | null;
  parentName?: string | null;
  name: string;
  code: string;
  description?: string | null;
  costCenter?: string | null;
  isActive: boolean;
}

export interface CreateDepartmentDto {
  branchId?: number | null;
  parentId?: number | null;
  name: string;
  code: string;
  description?: string | null;
  costCenter?: string | null;
}

export interface UpdateDepartmentDto extends CreateDepartmentDto {
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class DepartmentApiService {
  private departmentsUrl = `${environment.apiUrl}/departments`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<DepartmentDto[]> {
    return this.http.get<DepartmentDto[]>(this.departmentsUrl);
  }

  getById(id: number): Observable<DepartmentDto> {
    return this.http.get<DepartmentDto>(`${this.departmentsUrl}/${id}`);
  }

  create(dto: CreateDepartmentDto): Observable<DepartmentDto> {
    return this.http.post<DepartmentDto>(this.departmentsUrl, dto);
  }

  update(id: number, dto: UpdateDepartmentDto): Observable<DepartmentDto> {
    return this.http.put<DepartmentDto>(`${this.departmentsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.departmentsUrl}/${id}`);
  }
}

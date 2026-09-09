import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface RoleDto {
  id: number;
  name: string;
  description?: string | null;
  isSystemRole: boolean;
  permissionKeys: string[];
  userCount: number;
}

export interface CreateRoleDto {
  name: string;
  description?: string | null;
  permissionKeys: string[];
}

export interface UpdateRoleDto {
  name: string;
  description?: string | null;
  permissionKeys: string[];
}

@Injectable({ providedIn: 'root' })
export class RoleApiService {
  private rolesUrl = `${environment.apiUrl}/roles`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<RoleDto[]> {
    return this.http.get<RoleDto[]>(this.rolesUrl);
  }

  getById(id: number): Observable<RoleDto> {
    return this.http.get<RoleDto>(`${this.rolesUrl}/${id}`);
  }

  create(dto: CreateRoleDto): Observable<RoleDto> {
    return this.http.post<RoleDto>(this.rolesUrl, dto);
  }

  update(id: number, dto: UpdateRoleDto): Observable<RoleDto> {
    return this.http.put<RoleDto>(`${this.rolesUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.rolesUrl}/${id}`);
  }
}

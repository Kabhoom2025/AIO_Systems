import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface UserDto {
  id: number;
  name: string;
  email: string;
  roleId: number;
  roleName: string;
  branchId?: number | null;
  branchName?: string | null;
  employeeId?: number | null;
  employeeName?: string | null;
  isActive: boolean;
  lastLoginAt?: string | null;
  createdDate: string;
}

export interface CreateUserDto {
  name: string;
  email: string;
  password: string;
  roleId: number;
  branchId?: number | null;
  employeeId?: number | null;
}

export interface UpdateUserDto {
  name: string;
  roleId: number;
  branchId?: number | null;
  employeeId?: number | null;
  isActive: boolean;
}

export interface ResetUserPasswordDto {
  newPassword: string;
}

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

export interface PermissionDto {
  id: number;
  key: string;
  module: string;
  description: string;
}

export interface AuditLogDto {
  id: number;
  userId?: number | null;
  userName?: string | null;
  action: string;
  method: string;
  path: string;
  statusCode: number;
  ipAddress?: string | null;
  durationMs: number;
  createdDate: string;
}

export interface BranchLookupDto {
  id: number;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private usersUrl = `${environment.apiUrl}/users`;
  private rolesUrl = `${environment.apiUrl}/roles`;
  private permissionsUrl = `${environment.apiUrl}/permissions`;
  private auditLogsUrl = `${environment.apiUrl}/audit-logs`;

  constructor(private http: HttpClient) {}

  // Users
  getUsers(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(this.usersUrl);
  }

  getUser(id: number): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.usersUrl}/${id}`);
  }

  createUser(dto: CreateUserDto): Observable<UserDto> {
    return this.http.post<UserDto>(this.usersUrl, dto);
  }

  updateUser(id: number, dto: UpdateUserDto): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.usersUrl}/${id}`, dto);
  }

  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.usersUrl}/${id}`);
  }

  resetUserPassword(id: number, dto: ResetUserPasswordDto): Observable<void> {
    return this.http.post<void>(`${this.usersUrl}/${id}/reset-password`, dto);
  }

  // Roles
  getRoles(): Observable<RoleDto[]> {
    return this.http.get<RoleDto[]>(this.rolesUrl);
  }

  getRole(id: number): Observable<RoleDto> {
    return this.http.get<RoleDto>(`${this.rolesUrl}/${id}`);
  }

  createRole(dto: CreateRoleDto): Observable<RoleDto> {
    return this.http.post<RoleDto>(this.rolesUrl, dto);
  }

  updateRole(id: number, dto: UpdateRoleDto): Observable<RoleDto> {
    return this.http.put<RoleDto>(`${this.rolesUrl}/${id}`, dto);
  }

  deleteRole(id: number): Observable<void> {
    return this.http.delete<void>(`${this.rolesUrl}/${id}`);
  }

  // Permissions
  getPermissions(): Observable<PermissionDto[]> {
    return this.http.get<PermissionDto[]>(this.permissionsUrl);
  }

  // Audit logs
  getAuditLogsPaged(page = 1, pageSize = 50, search?: string | null): Observable<PagedResult<AuditLogDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    return this.http.get<PagedResult<AuditLogDto>>(this.auditLogsUrl, { params });
  }

  // Branch lookup (used by user create/edit dialogs)
  getBranches(): Observable<BranchLookupDto[]> {
    return this.http.get<BranchLookupDto[]>(`${environment.apiUrl}/branches`);
  }
}

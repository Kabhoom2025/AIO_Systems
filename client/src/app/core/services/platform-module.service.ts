import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreatePlatformModuleRequest,
  OrgModuleAssignmentRequest,
  OrgModuleStatus,
  PlatformModule,
  UpdatePlatformModuleRequest,
} from '../models/platform-module.model';

@Injectable({ providedIn: 'root' })
export class PlatformModuleService {
  private readonly base = `${environment.apiUrl}/platform-modules`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<PlatformModule[]> {
    return this.http.get<PlatformModule[]>(this.base);
  }

  getById(id: number): Observable<PlatformModule> {
    return this.http.get<PlatformModule>(`${this.base}/${id}`);
  }

  create(request: CreatePlatformModuleRequest): Observable<PlatformModule> {
    return this.http.post<PlatformModule>(this.base, request);
  }

  update(id: number, request: UpdatePlatformModuleRequest): Observable<PlatformModule> {
    return this.http.put<PlatformModule>(`${this.base}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  getAllOrgStatus(): Observable<OrgModuleStatus[]> {
    return this.http.get<OrgModuleStatus[]>(`${this.base}/org-status`);
  }

  getOrgStatus(orgId: number): Observable<OrgModuleStatus> {
    return this.http.get<OrgModuleStatus>(`${this.base}/org-status/${orgId}`);
  }

  assignToOrg(request: OrgModuleAssignmentRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/assign`, request);
  }
}

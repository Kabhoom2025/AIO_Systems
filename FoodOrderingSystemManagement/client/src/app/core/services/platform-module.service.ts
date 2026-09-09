import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PlatformModule, OrgModuleStatus, OrgModuleAssignment, CreatePlatformModule, UpdatePlatformModule } from '../models/platform-module.model';

@Injectable({ providedIn: 'root' })
export class PlatformModuleService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/platform-modules`;

  getAll(): Observable<PlatformModule[]> {
    return this.http.get<PlatformModule[]>(this.base);
  }

  getById(id: number): Observable<PlatformModule> {
    return this.http.get<PlatformModule>(`${this.base}/${id}`);
  }

  create(dto: CreatePlatformModule): Observable<PlatformModule> {
    return this.http.post<PlatformModule>(this.base, dto);
  }

  update(id: number, dto: UpdatePlatformModule): Observable<PlatformModule> {
    return this.http.put<PlatformModule>(`${this.base}/${id}`, dto);
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

  assignModules(dto: OrgModuleAssignment): Observable<void> {
    return this.http.post<void>(`${this.base}/assign`, dto);
  }
}

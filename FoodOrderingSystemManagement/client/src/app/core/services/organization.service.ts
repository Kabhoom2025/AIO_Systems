import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Organization, CreateOrganizationRequest, UpdateOrganizationRequest, OrgUser, CreateOrgAdminRequest, OrgReport, License, UpsertLicenseRequest } from '../models/organization.model';
import { Settings } from '../models/settings.model';

@Injectable({ providedIn: 'root' })
export class OrganizationService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/organizations`;

  getAll(): Observable<Organization[]> { return this.http.get<Organization[]>(this.base); }
  getById(id: number): Observable<Organization> { return this.http.get<Organization>(`${this.base}/${id}`); }
  create(req: CreateOrganizationRequest): Observable<Organization> { return this.http.post<Organization>(this.base, req); }
  update(id: number, req: UpdateOrganizationRequest): Observable<Organization> { return this.http.put<Organization>(`${this.base}/${id}`, req); }
  delete(id: number): Observable<void> { return this.http.delete<void>(`${this.base}/${id}`); }

  getOrgUsers(orgId: number): Observable<OrgUser[]> { return this.http.get<OrgUser[]>(`${this.base}/${orgId}/users`); }
  createOrgAdmin(orgId: number, req: CreateOrgAdminRequest): Observable<OrgUser> { return this.http.post<OrgUser>(`${this.base}/${orgId}/admins`, req); }
  deleteOrgUser(orgId: number, userId: number): Observable<void> { return this.http.delete<void>(`${this.base}/${orgId}/users/${userId}`); }

  getOrgReport(orgId: number): Observable<OrgReport> { return this.http.get<OrgReport>(`${this.base}/${orgId}/reports`); }
  getOrgSettings(orgId: number): Observable<Settings> { return this.http.get<Settings>(`${this.base}/${orgId}/settings`); }
  updateOrgSettings(orgId: number, payload: Partial<Settings>): Observable<Settings> { return this.http.put<Settings>(`${this.base}/${orgId}/settings`, payload); }

  getAllLicenses(): Observable<License[]> { return this.http.get<License[]>(`${this.base}/licenses`); }
  getOrgLicense(orgId: number): Observable<License> { return this.http.get<License>(`${this.base}/${orgId}/license`); }
  upsertOrgLicense(orgId: number, req: UpsertLicenseRequest): Observable<License> { return this.http.put<License>(`${this.base}/${orgId}/license`, req); }
  deleteOrgLicense(orgId: number): Observable<void> { return this.http.delete<void>(`${this.base}/${orgId}/license`); }
}

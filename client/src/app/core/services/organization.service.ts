import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateOrgAdminRequest,
  CreateOrganizationRequest,
  License,
  OrgReport,
  OrgUser,
  Organization,
  UpdateOrganizationRequest,
  UpsertLicenseRequest,
} from '../models/organization.model';

@Injectable({ providedIn: 'root' })
export class OrganizationService {
  private readonly base = `${environment.apiUrl}/organizations`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Organization[]> {
    return this.http.get<Organization[]>(this.base);
  }

  getById(id: number): Observable<Organization> {
    return this.http.get<Organization>(`${this.base}/${id}`);
  }

  create(request: CreateOrganizationRequest): Observable<Organization> {
    return this.http.post<Organization>(this.base, request);
  }

  update(id: number, request: UpdateOrganizationRequest): Observable<Organization> {
    return this.http.put<Organization>(`${this.base}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  getOrgUsers(orgId: number): Observable<OrgUser[]> {
    return this.http.get<OrgUser[]>(`${this.base}/${orgId}/users`);
  }

  createOrgAdmin(orgId: number, request: CreateOrgAdminRequest): Observable<OrgUser> {
    return this.http.post<OrgUser>(`${this.base}/${orgId}/admins`, request);
  }

  getAllLicenses(): Observable<License[]> {
    return this.http.get<License[]>(`${this.base}/licenses`);
  }

  getLicense(orgId: number): Observable<License> {
    return this.http.get<License>(`${this.base}/${orgId}/license`);
  }

  upsertLicense(orgId: number, request: UpsertLicenseRequest): Observable<License> {
    return this.http.put<License>(`${this.base}/${orgId}/license`, request);
  }

  deleteLicense(orgId: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${orgId}/license`);
  }

  // Restaurant business data — proxied through the gateway's restaurant-route,
  // which strips /restaurant before forwarding to FoodOrder.API. FoodOrder still
  // owns its own OrganizationController with this exact /reports sub-route.
  getRestaurantOrgReport(orgId: number): Observable<OrgReport> {
    return this.http.get<OrgReport>(`${environment.apiUrl}/restaurant/organizations/${orgId}/reports`);
  }
}

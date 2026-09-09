import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ExpiryAlert, Medicine, Prescription } from '../models/pharmacy.model';

/**
 * Reads pharmacy business data for a tenant through the gateway's pharmacy-route
 * (proxies to Pharmacy.API, prefix-stripped). Read-only from the super-admin side —
 * mutations belong to the Pharmacy app itself.
 */
@Injectable({ providedIn: 'root' })
export class PharmacyDataService {
  private readonly base = `${environment.apiUrl}/pharmacy`;

  constructor(private http: HttpClient) {}

  getMedicines(orgId: number): Observable<Medicine[]> {
    return this.http.get<Medicine[]>(`${this.base}/medicines/org/${orgId}`);
  }

  getExpiryAlerts(orgId: number, days = 90): Observable<ExpiryAlert[]> {
    return this.http.get<ExpiryAlert[]>(`${this.base}/medicines/expiry-alerts/org/${orgId}`, {
      params: { days },
    });
  }

  getPrescriptions(orgId: number): Observable<Prescription[]> {
    return this.http.get<Prescription[]>(`${this.base}/prescriptions/org/${orgId}`);
  }
}

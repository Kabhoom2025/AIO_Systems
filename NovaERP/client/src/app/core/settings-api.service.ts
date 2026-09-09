import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface OrganizationSettingsDto {
  id: number;
  defaultLanguageCode: string;
  defaultCurrencyCode: string;
  defaultTimezone: string;
  dateFormat: string;
  timeFormat: string;
  fiscalYearStartMonth: number;
  invoiceNumberPrefix: string;
  taxInclusivePricing: boolean;
}

export interface UpdateOrganizationSettingsDto {
  defaultLanguageCode: string;
  defaultCurrencyCode: string;
  defaultTimezone: string;
  dateFormat: string;
  timeFormat: string;
  fiscalYearStartMonth: number;
  invoiceNumberPrefix: string;
  taxInclusivePricing: boolean;
}

@Injectable({ providedIn: 'root' })
export class SettingsApiService {
  private settingsUrl = `${environment.apiUrl}/settings`;

  constructor(private http: HttpClient) {}

  get(): Observable<OrganizationSettingsDto> {
    return this.http.get<OrganizationSettingsDto>(this.settingsUrl);
  }

  update(dto: UpdateOrganizationSettingsDto): Observable<OrganizationSettingsDto> {
    return this.http.put<OrganizationSettingsDto>(this.settingsUrl, dto);
  }
}

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface OrganizationLanguageDto {
  id: number;
  languageCode: string;
  languageName: string;
  isDefault: boolean;
}

export interface UpdateOrganizationLanguagesDto {
  languageCodes: string[];
  defaultLanguageCode: string;
}

@Injectable({ providedIn: 'root' })
export class OrganizationLanguageApiService {
  private orgLanguagesUrl = `${environment.apiUrl}/organizations/languages`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<OrganizationLanguageDto[]> {
    return this.http.get<OrganizationLanguageDto[]>(this.orgLanguagesUrl);
  }

  update(dto: UpdateOrganizationLanguagesDto): Observable<OrganizationLanguageDto[]> {
    return this.http.put<OrganizationLanguageDto[]>(this.orgLanguagesUrl, dto);
  }
}

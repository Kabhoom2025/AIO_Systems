import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface FeatureToggleDto {
  id: number;
  moduleKey: string;
  isEnabled: boolean;
}

export interface UpdateFeatureToggleDto {
  moduleKey: string;
  isEnabled: boolean;
}

@Injectable({ providedIn: 'root' })
export class FeatureToggleApiService {
  private togglesUrl = `${environment.apiUrl}/feature-toggles`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<FeatureToggleDto[]> {
    return this.http.get<FeatureToggleDto[]>(this.togglesUrl);
  }

  update(moduleKey: string, dto: UpdateFeatureToggleDto): Observable<FeatureToggleDto> {
    return this.http.put<FeatureToggleDto>(`${this.togglesUrl}/${moduleKey}`, dto);
  }
}

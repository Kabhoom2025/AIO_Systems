import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { Settings, UpdateSettingsRequest } from '../models/settings.model';

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private readonly apiUrl = `${environment.apiUrl}/settings`;
  private _settings = signal<Settings | null>(null);
  readonly settings = this._settings.asReadonly();

  constructor(private http: HttpClient) {}

  load() {
    return this.http.get<ApiResponse<Settings>>(this.apiUrl).pipe(
      tap((res) => { if (res.success) this._settings.set(res.data); })
    );
  }

  getSettings(): Observable<Settings> {
    return this.http.get<ApiResponse<Settings>>(this.apiUrl).pipe(
      tap(res => { if (res.success) this._settings.set(res.data ?? null); }),
      map(res => res.data as Settings)
    );
  }

  updateSettings(payload: UpdateSettingsRequest | Settings): Observable<Settings> {
    return this.http.put<ApiResponse<Settings>>(this.apiUrl, payload).pipe(
      tap(res => { if (res.success) this._settings.set(res.data ?? null); }),
      map(res => res.data as Settings)
    );
  }
}

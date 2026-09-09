import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { UpdateUserSettingsRequest, UserSettings } from '../models/settings.model';

const DEFAULT_SETTINGS: UserSettings = {
  language: 'en-US',
  voice: 'default',
  speechRate: 1,
  pitch: 1,
  volume: 1,
  autoSpeak: false,
  voiceMode: false,
  theme: 'system',
  aiProvider: null,
  hasCustomAiApiKey: false,
  aiModel: null
};

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private readonly baseUrl = `${environment.apiBaseUrl}/settings`;
  readonly settings = signal<UserSettings>(DEFAULT_SETTINGS);

  constructor(private readonly http: HttpClient) {}

  load(): Observable<UserSettings> {
    return this.http.get<UserSettings>(this.baseUrl).pipe(tap((settings) => this.settings.set(settings)));
  }

  update(request: UpdateUserSettingsRequest): Observable<UserSettings> {
    return this.http.put<UserSettings>(this.baseUrl, request).pipe(tap((settings) => this.settings.set(settings)));
  }
}

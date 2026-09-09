import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface NotificationChannelSettingsDto {
  id: number;
  organizationId: number;

  smtpHost?: string | null;
  smtpPort?: number | null;
  smtpUsername?: string | null;
  smtpFromEmail?: string | null;
  smtpFromName?: string | null;
  smtpUseSsl: boolean;

  smsApiUrl?: string | null;
  smsSenderId?: string | null;

  pushApiUrl?: string | null;
}

export interface UpdateNotificationChannelSettingsDto {
  smtpHost?: string | null;
  smtpPort?: number | null;
  smtpUsername?: string | null;
  smtpPassword?: string | null;
  smtpFromEmail?: string | null;
  smtpFromName?: string | null;
  smtpUseSsl: boolean;

  smsApiUrl?: string | null;
  smsApiKey?: string | null;
  smsSenderId?: string | null;

  pushApiUrl?: string | null;
  pushServerKey?: string | null;
}

@Injectable({ providedIn: 'root' })
export class NotificationChannelSettingsApiService {
  private settingsUrl = `${environment.apiUrl}/notification-channel-settings`;

  constructor(private http: HttpClient) {}

  get(): Observable<NotificationChannelSettingsDto> {
    return this.http.get<NotificationChannelSettingsDto>(this.settingsUrl);
  }

  update(dto: UpdateNotificationChannelSettingsDto): Observable<NotificationChannelSettingsDto> {
    return this.http.put<NotificationChannelSettingsDto>(this.settingsUrl, dto);
  }
}

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface NotificationDto {
  id: number;
  userId?: number | null;
  title: string;
  message: string;
  type: string;
  link?: string | null;
  isRead: boolean;
  createdDate: string;
}

export interface TestSendNotificationDto {
  channel: 'Email' | 'Sms' | 'Push';
  to: string;
  message: string;
  subject?: string | null;
}

export interface SendResultDto {
  success: boolean;
  status: string;
  error?: string | null;
}

@Injectable({ providedIn: 'root' })
export class NotificationApiService {
  private notificationsUrl = `${environment.apiUrl}/notifications`;

  constructor(private http: HttpClient) {}

  getMine(take = 50): Observable<NotificationDto[]> {
    const params = new HttpParams().set('take', take);
    return this.http.get<NotificationDto[]>(this.notificationsUrl, { params });
  }

  getUnreadCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.notificationsUrl}/unread-count`);
  }

  markRead(id: number): Observable<void> {
    return this.http.post<void>(`${this.notificationsUrl}/${id}/read`, {});
  }

  markAllRead(): Observable<void> {
    return this.http.post<void>(`${this.notificationsUrl}/read-all`, {});
  }

  testSend(dto: TestSendNotificationDto): Observable<SendResultDto> {
    return this.http.post<SendResultDto>(`${this.notificationsUrl}/test-send`, dto);
  }
}

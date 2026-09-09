import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { getAuthHeaders } from './auth.helper';

export interface AppNotification {
  id: number;
  type: string;
  title: string;
  message: string;
  relatedEntityType?: string;
  relatedEntityId?: number;
  isRead: boolean;
  createdDate: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationsApiService {
  constructor(private http: HttpClient) {}

  getList(orgId: number) {
    return this.http.get<AppNotification[]>(`${environment.apiUrl}/notifications/org/${orgId}`, {
      headers: getAuthHeaders()
    });
  }

  getUnreadCount(orgId: number) {
    return this.http.get<{ count: number }>(`${environment.apiUrl}/notifications/unread-count/org/${orgId}`, {
      headers: getAuthHeaders()
    });
  }

  markRead(id: number) {
    return this.http.post(`${environment.apiUrl}/notifications/${id}/read`, {}, {
      headers: getAuthHeaders()
    });
  }

  markAllRead(orgId: number) {
    return this.http.post(`${environment.apiUrl}/notifications/org/${orgId}/read-all`, {}, {
      headers: getAuthHeaders()
    });
  }
}

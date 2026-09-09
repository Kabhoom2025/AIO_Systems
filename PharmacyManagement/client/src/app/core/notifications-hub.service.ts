import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { AppNotification } from './notifications-api.service';

export interface NotificationsUpdatedPayload {
  unreadCount: number;
  notifications: AppNotification[];
}

@Injectable({ providedIn: 'root' })
export class NotificationsHubService {
  private connection?: signalR.HubConnection;

  start() {
    if (this.connection) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.signalrUrl}/hubs/notifications`, {
        accessTokenFactory: () => localStorage.getItem(environment.tokenKey) ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.connection.start().catch(() => {
      // Connection failures are non-fatal — the shell's fallback poll keeps the badge fresh.
    });
  }

  stop() {
    this.connection?.stop();
    this.connection = undefined;
  }

  onNotificationsUpdated(callback: (payload: NotificationsUpdatedPayload) => void) {
    this.connection?.on('notificationsUpdated', callback);
  }
}

import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { NotificationDto } from './notification-api.service';

/**
 * Thin wrapper around the SignalR client connecting to NovaERP.API's NotificationsHub.
 * The hub is mapped at `/hubs/notifications` (see NovaERP.API/Program.cs) and is [Authorize]-protected;
 * since browser EventSource/WebSocket clients cannot set an Authorization header, the JWT is passed
 * as the `access_token` query string parameter, which Program.cs's JwtBearerEvents.OnMessageReceived
 * reads specifically for requests under `/hubs/notifications`.
 */
@Injectable({ providedIn: 'root' })
export class NotificationHubService {
  private hubConnection: signalR.HubConnection | null = null;
  private notificationReceived = new Subject<NotificationDto>();

  readonly notification$ = this.notificationReceived.asObservable();

  connect(): void {
    if (this.hubConnection) return;

    const token = localStorage.getItem(environment.tokenKey) ?? '';

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.signalrUrl}/hubs/notifications`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('notification', (payload: NotificationDto) => {
      this.notificationReceived.next(payload);
    });

    this.hubConnection.start().catch(err => console.error('NotificationsHub connection failed:', err));
  }

  disconnect(): void {
    this.hubConnection?.stop();
    this.hubConnection = null;
  }
}

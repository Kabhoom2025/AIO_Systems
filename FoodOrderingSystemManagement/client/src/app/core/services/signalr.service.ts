import { Injectable, inject, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AuthService } from '../authentication/auth.service';
import { environment } from '../../../environments/environment';

export interface OrderStatusChangedEvent {
  orderId: number;
  orderNumber: string;
  status: string;
  tableNumber: number | null;
}

export interface OrderCreatedEvent {
  orderId: number;
  orderNumber: string;
  tableNumber: number | null;
  items: { name: string; qty: number; addOnNotes: string | null }[];
}

export interface OrderRejectedEvent {
  orderId: number;
  orderNumber: string;
  tableNumber: number | null;
  reason: string;
}

export interface OrderItemsRejectedEvent {
  orderId: number;
  orderNumber: string;
  tableNumber: number | null;
  orderCancelled: boolean;
  rejectedItems: string[];
  reason: string;
}

export interface LedgerEntryCreatedEvent {
  id: number;
  date: string;
  type: string;
  amount: number;
  category: string;
  note: string | null;
  createdBy: string;
}

@Injectable({ providedIn: 'root' })
export class SignalRService implements OnDestroy {
  private authService = inject(AuthService);

  private hubUrl = environment.apiUrl.replace('/api', '') + '/hubs/orders';
  private connection: signalR.HubConnection | null = null;

  readonly orderStatusChanged$      = new Subject<OrderStatusChangedEvent>();
  readonly orderCreated$            = new Subject<OrderCreatedEvent>();
  readonly orderRejectedByKitchen$  = new Subject<OrderRejectedEvent>();
  readonly orderItemsRejected$      = new Subject<OrderItemsRejectedEvent>();
  readonly ledgerEntryCreated$      = new Subject<LedgerEntryCreatedEvent>();

  connected = false;

  async startConnection(): Promise<void> {
    if (this.connection) {
      const s = this.connection.state;
      if (s === signalR.HubConnectionState.Connected   ||
          s === signalR.HubConnectionState.Connecting  ||
          s === signalR.HubConnectionState.Reconnecting) return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => this.authService.getToken() ?? '',
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.connection.on('OrderStatusChanged', (payload: OrderStatusChangedEvent) => {
      this.orderStatusChanged$.next(payload);
    });

    this.connection.on('OrderCreated', (payload: OrderCreatedEvent) => {
      this.orderCreated$.next(payload);
    });

    this.connection.on('OrderRejectedByKitchen', (payload: OrderRejectedEvent) => {
      this.orderRejectedByKitchen$.next(payload);
    });

    this.connection.on('OrderItemsRejected', (payload: OrderItemsRejectedEvent) => {
      this.orderItemsRejected$.next(payload);
    });

    this.connection.on('LedgerEntryCreated', (payload: LedgerEntryCreatedEvent) => {
      this.ledgerEntryCreated$.next(payload);
    });

    try {
      await this.connection.start();
      this.connected = true;
    } catch (err) {
      console.error('SignalR connection error:', err);
    }
  }

  async joinGroup(group: string): Promise<void> {
    if (!this.connection) return;
    await this.connection.invoke('JoinGroup', group);
  }

  async stopConnection(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connected = false;
    }
  }

  ngOnDestroy(): void {
    this.stopConnection();
  }
}

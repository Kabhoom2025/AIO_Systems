import { Injectable, signal, computed, inject } from '@angular/core';
import { OrderService } from './order.service';
import { SignalRService, OrderStatusChangedEvent, OrderRejectedEvent } from './signalr.service';

export interface AppNotification {
  id: string;
  type: 'ready' | 'rejected' | 'info';
  message: string;
  reason?: string;
  timestamp: Date;
  read: boolean;
  orderNumber?: string;
  tableNumber?: number | null;
  orderId?: number;
  pickedUp?: boolean;
}

@Injectable({ providedIn: 'root' })
export class NotificationStoreService {
  private _list        = signal<AppNotification[]>([]);
  private orderService = inject(OrderService);
  private signalR      = inject(SignalRService);

  readonly notifications = this._list.asReadonly();
  readonly unreadCount   = computed(() => this._list().filter(n => !n.read).length);

  constructor() {
    // Kitchen Ready — green notification
    this.signalR.orderStatusChanged$.subscribe((ev: OrderStatusChangedEvent) => {
      if (ev.status === 'KitchenReady') {
        const tableInfo = ev.tableNumber ? `Table ${ev.tableNumber}` : 'Takeaway';
        this.addOrRefresh({
          type:        'ready',
          message:     `Order #${ev.orderNumber} (${tableInfo}) is ready for pickup!`,
          orderNumber: ev.orderNumber,
          tableNumber: ev.tableNumber,
          orderId:     ev.orderId,
        });
      }
    });

    // Kitchen Rejected — red notification with reason
    this.signalR.orderRejectedByKitchen$.subscribe((ev: OrderRejectedEvent) => {
      const tableInfo = ev.tableNumber ? `Table ${ev.tableNumber}` : 'Takeaway';
      this.add({
        type:        'rejected',
        message:     `Order #${ev.orderNumber} (${tableInfo}) was REJECTED by the kitchen.`,
        reason:      ev.reason,
        orderNumber: ev.orderNumber,
        tableNumber: ev.tableNumber,
        orderId:     ev.orderId,
      });
    });

    // Start the hub connection (idempotent — safe to call multiple times).
    this.signalR.startConnection();
  }

  add(partial: Omit<AppNotification, 'id' | 'timestamp' | 'read'>): void {
    const entry: AppNotification = {
      ...partial,
      id:        crypto.randomUUID(),
      timestamp: new Date(),
      read:      false,
      pickedUp:  partial.pickedUp ?? false,
    };
    this._list.update(list => [entry, ...list].slice(0, 50));
  }

  /** Add or refresh — for 'ready' notifications only (deduplicates by orderId). */
  addOrRefresh(partial: Omit<AppNotification, 'id' | 'timestamp' | 'read'>): void {
    const existing = this._list().find(n => n.orderId === partial.orderId && !n.pickedUp);
    if (existing) {
      this._list.update(list =>
        list.map(n => n.id === existing.id
          ? { ...n, message: partial.message, read: false, timestamp: new Date() }
          : n)
      );
    } else {
      this.add(partial);
    }
  }

  markAllRead(): void {
    this._list.update(list => list.map(n => ({ ...n, read: true })));
  }

  dismiss(id: string): void {
    this._list.update(list => list.filter(n => n.id !== id));
  }

  clearAll(): void {
    this._list.set([]);
  }

  pickUpOrder(notifId: string): void {
    const notif = this._list().find(n => n.id === notifId);
    if (!notif?.orderId || notif.pickedUp) return;

    this.orderService.markPickedUp(notif.orderId).subscribe({
      next: () => {
        this._list.update(list =>
          list.map(n => n.id === notifId ? { ...n, pickedUp: true } : n)
        );
      },
      error: () => {},
    });
  }
}

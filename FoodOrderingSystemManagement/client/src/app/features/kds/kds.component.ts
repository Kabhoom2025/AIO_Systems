import { Component, inject, signal, computed, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subscription } from 'rxjs';
import { HasPermissionDirective } from '../../core/directives/has-permission.directive';
import { environment } from '../../../environments/environment';
import { SignalRService } from '../../core/services/signalr.service';
import { NotificationService } from '../../shared/services/notification.service';
import { PrinterService } from '../../core/services/printer.service';
import { buildKotEscPos } from '../../core/utils/escpos-builder';

export interface KdsItem { foodItemId: number; name: string; qty: number; addOnNotes: string | null; }

export interface KdsOrder {
  id: number;
  orderNumber: string;
  tableNumber: number | null;
  grandTotal: number;
  createdAt: string;
  elapsedMs: number;
  marking: boolean;
  preparingItems: KdsItem[];
  readyItems: KdsItem[];
}

const DELAY_OPTIONS = [
  { label: '+5 min',  mins: 5  },
  { label: '+10 min', mins: 10 },
  { label: '+15 min', mins: 15 },
  { label: '+30 min', mins: 30 },
];

@Component({
  selector: 'app-kds',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatTooltipModule, HasPermissionDirective],
  templateUrl: './kds.component.html',
  styleUrl: './kds.component.scss',
})
export class KdsComponent implements OnInit, OnDestroy {
  private http    = inject(HttpClient);
  private signalR = inject(SignalRService);
  private notify  = inject(NotificationService);
  private printerService = inject(PrinterService);

  orders  = signal<KdsOrder[]>([]);
  loading = signal(true);

  // ── Local KDS state (session-only) ──────────────────────────────────────
  acceptedIds    = signal<Set<number>>(new Set());
  kitchenNotes   = signal<Map<number, string>>(new Map());
  /** Per-order set of foodItemIds the chef has marked for rejection */
  rejectedItems  = signal<Map<number, Set<number>>>(new Map());
  delayedOrders  = signal<Map<number, Date>>(new Map());
  acceptedAt     = signal<Map<number, number>>(new Map()); // id → timestamp
  /** Per-order reason text for partial rejection */
  itemRejectReasons = signal<Map<number, string>>(new Map());
  sendingReject     = signal<number | null>(null); // orderId being submitted

  // ── Lanes ───────────────────────────────────────────────────────────────
  newLane = computed(() =>
    this.orders().filter(o => !this.acceptedIds().has(o.id) && o.preparingItems.length > 0)
  );
  preparingLane = computed(() =>
    this.orders().filter(o => this.acceptedIds().has(o.id) && o.preparingItems.length > 0)
  );
  readyLane = computed(() =>
    this.orders().filter(o => o.readyItems.length > 0)
  );

  // ── Modals ───────────────────────────────────────────────────────────────
  noteModal    = signal<KdsOrder | null>(null);
  noteInput    = '';
  delayModal   = signal<KdsOrder | null>(null);
  printOrder   = signal<KdsOrder | null>(null);
  rejectModal  = signal<KdsOrder | null>(null);
  rejectReason = '';
  rejecting    = signal(false);

  readonly delayOptions = DELAY_OPTIONS;
  readonly now = signal(Date.now());

  private subs:   Subscription[] = [];
  private ticker: ReturnType<typeof setInterval> | null = null;

  ngOnInit(): void {
    this.loadOrders();
    this.startSignalR();
    this.ticker = setInterval(() => {
      this.tickElapsed();
      this.now.set(Date.now());
    }, 10_000);
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
    if (this.ticker) clearInterval(this.ticker);
  }

  // ── Data loading ──────────────────────────────────────────────────────────
  private loadOrders(): void {
    this.http.get<any>(`${environment.apiUrl}/order/pending`).subscribe({
      next: res => {
        const raw: any[] = res.data ?? [];
        const kdsOrders = raw
          .filter(o => o.status === 'Pending' || o.status === 'KitchenReady')
          .map(o => this.mapOrder(o))
          .filter(o => o.preparingItems.length > 0 || o.readyItems.length > 0);
        this.orders.set(kdsOrders);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  private startSignalR(): void {
    // Ensure the hub connection is up — idempotent, safe to call even if already connected.
    this.signalR.startConnection();
    this.subs.push(
      this.signalR.orderCreated$.subscribe(() => this.loadOrders()),
      this.signalR.orderStatusChanged$.subscribe(() => this.loadOrders()),
    );
  }

  // ── Accept / Start Preparing ──────────────────────────────────────────────
  acceptOrder(order: KdsOrder): void {
    this.acceptedIds.update(s => new Set([...s, order.id]));
    this.acceptedAt.update(m => new Map([...m, [order.id, Date.now()]]));
    this.notify.success(`Order ${order.orderNumber} accepted — starting prep!`);
  }

  // ── Reject order — open modal ─────────────────────────────────────────────
  openRejectModal(order: KdsOrder): void {
    this.rejectReason = '';
    this.rejectModal.set(order);
  }
  closeRejectModal(): void { this.rejectModal.set(null); }

  confirmReject(): void {
    const order = this.rejectModal();
    if (!order) return;
    this.rejecting.set(true);
    this.http.patch<any>(`${environment.apiUrl}/order/${order.id}/kitchen-reject`,
      { reason: this.rejectReason.trim() || null }
    ).subscribe({
      next: () => {
        this.rejecting.set(false);
        this.rejectModal.set(null);
        this.orders.update(list => list.filter(o => o.id !== order.id));
        this.notify.error(`Order ${order.orderNumber} rejected — cashier notified.`);
      },
      error: err => {
        this.rejecting.set(false);
        this.notify.error(err?.error?.message ?? 'Failed to reject order.');
      },
    });
  }

  // ── Reject single item ────────────────────────────────────────────────────
  toggleItemReject(orderId: number, foodItemId: number): void {
    this.rejectedItems.update(map => {
      const next = new Map(map);
      const set  = new Set(next.get(orderId) ?? []);
      set.has(foodItemId) ? set.delete(foodItemId) : set.add(foodItemId);
      if (set.size === 0) next.delete(orderId); else next.set(orderId, set);
      return next;
    });
  }

  isItemRejected(orderId: number, foodItemId: number): boolean {
    return this.rejectedItems().get(orderId)?.has(foodItemId) ?? false;
  }

  rejectedCount(orderId: number): number {
    return this.rejectedItems().get(orderId)?.size ?? 0;
  }

  setItemRejectReason(orderId: number, val: string): void {
    this.itemRejectReasons.update(m => new Map(m).set(orderId, val));
  }

  getItemRejectReason(orderId: number): string {
    return this.itemRejectReasons().get(orderId) ?? '';
  }

  sendItemRejections(order: KdsOrder): void {
    const ids = Array.from(this.rejectedItems().get(order.id) ?? []);
    if (!ids.length) return;
    const reason = this.getItemRejectReason(order.id).trim() || null;
    this.sendingReject.set(order.id);
    this.http.patch<any>(`${environment.apiUrl}/order/${order.id}/reject-items`,
      { foodItemIds: ids, reason }
    ).subscribe({
      next: res => {
        this.sendingReject.set(null);
        // Clear rejection state for this order
        this.rejectedItems.update(m => { const n = new Map(m); n.delete(order.id); return n; });
        this.itemRejectReasons.update(m => { const n = new Map(m); n.delete(order.id); return n; });
        if (res.data?.orderCancelled) {
          this.orders.update(list => list.filter(o => o.id !== order.id));
          this.notify.error(`Order ${order.orderNumber} fully cancelled — all items unavailable.`);
        } else {
          this.notify.success(`Rejected items removed. Cashier notified.`);
          this.loadOrders(); // reload to get updated item list
        }
      },
      error: err => {
        this.sendingReject.set(null);
        this.notify.error(err?.error?.message ?? 'Failed to reject items.');
      },
    });
  }

  // ── Mark Ready ────────────────────────────────────────────────────────────
  markReady(order: KdsOrder): void {
    this.orders.update(list => list.map(o => o.id === order.id ? { ...o, marking: true } : o));
    this.http.patch<any>(`${environment.apiUrl}/order/${order.id}/mark-ready`, {}).subscribe({
      next: () => {
        this.notify.success(`Order ${order.orderNumber} is ready for pickup!`);
        this.orders.update(list =>
          list.map(o => o.id === order.id ? {
            ...o, marking: false,
            readyItems: [...o.readyItems, ...o.preparingItems],
            preparingItems: [],
          } : o)
        );
      },
      error: err => {
        this.notify.error(err?.error?.message ?? 'Failed to mark order.');
        this.orders.update(list => list.map(o => o.id === order.id ? { ...o, marking: false } : o));
      },
    });
  }

  // ── Mark Served ───────────────────────────────────────────────────────────
  markServed(order: KdsOrder): void {
    this.orders.update(list => list.map(o => o.id === order.id ? { ...o, marking: true } : o));
    this.http.patch<any>(`${environment.apiUrl}/order/${order.id}/mark-served`, {}).subscribe({
      next: () => {
        this.notify.success(`Order ${order.orderNumber} delivered to table!`);
        this.orders.update(list =>
          list
            .map(o => o.id === order.id ? { ...o, marking: false, readyItems: [] } : o)
            .filter(o => o.preparingItems.length > 0 || o.readyItems.length > 0)
        );
      },
      error: err => {
        this.notify.error(err?.error?.message ?? 'Failed to mark as served.');
        this.orders.update(list => list.map(o => o.id === order.id ? { ...o, marking: false } : o));
      },
    });
  }

  // ── Delay ─────────────────────────────────────────────────────────────────
  openDelayModal(order: KdsOrder): void { this.delayModal.set(order); }
  closeDelayModal(): void { this.delayModal.set(null); }

  applyDelay(order: KdsOrder, mins: number): void {
    const until = new Date(Date.now() + mins * 60_000);
    this.delayedOrders.update(m => new Map([...m, [order.id, until]]));
    this.notify.success(`Order ${order.orderNumber} delayed by ${mins} min.`);
    this.delayModal.set(null);
  }

  cancelDelay(orderId: number): void {
    this.delayedOrders.update(m => { const n = new Map(m); n.delete(orderId); return n; });
  }

  getDelayLabel(orderId: number): string | null {
    const until = this.delayedOrders().get(orderId);
    if (!until) return null;
    const rem = Math.max(0, Math.ceil((until.getTime() - Date.now()) / 60_000));
    return rem > 0 ? `Delayed ${rem}m` : 'Delay expired';
  }

  // ── Kitchen Notes ─────────────────────────────────────────────────────────
  openNoteModal(order: KdsOrder): void {
    this.noteInput = this.kitchenNotes().get(order.id) ?? '';
    this.noteModal.set(order);
  }
  closeNoteModal(): void { this.noteModal.set(null); }

  saveNote(): void {
    const order = this.noteModal();
    if (!order) return;
    const text = this.noteInput.trim();
    this.kitchenNotes.update(m => {
      const n = new Map(m);
      text ? n.set(order.id, text) : n.delete(order.id);
      return n;
    });
    this.notify.success('Kitchen note saved.');
    this.noteModal.set(null);
  }

  getNote(orderId: number): string { return this.kitchenNotes().get(orderId) ?? ''; }

  // ── Print Kitchen Slip ────────────────────────────────────────────────────
  openPrint(order: KdsOrder): void { this.printOrder.set(order); }
  closePrint(): void { this.printOrder.set(null); }

  async printSlip(): Promise<void> {
    const order = this.printOrder();
    if (!order) return;
    const items = [...order.preparingItems, ...order.readyItems];
    const kot = buildKotEscPos(order.orderNumber, order.tableNumber, items);
    const printed = await this.printerService.printReceipt(kot);
    if (!printed) window.print();
  }

  getPrepTime(orderId: number): string {
    const ts = this.acceptedAt().get(orderId);
    if (!ts) return '—';
    const m = Math.floor((Date.now() - ts) / 60_000);
    return m < 1 ? '< 1 min' : `${m} min`;
  }

  // ── Helpers ───────────────────────────────────────────────────────────────
  private mapOrder(o: any): KdsOrder {
    const nonDelivered = (o.items ?? []).filter((i: any) => !i.isDelivered);
    return {
      id:             o.id,
      orderNumber:    o.orderNumber,
      tableNumber:    o.tableNumber ?? null,
      grandTotal:     o.grandTotal,
      createdAt:      o.createdDate ?? o.orderDate,
      preparingItems: nonDelivered
        .filter((i: any) => !i.isKitchenReady)
        .map((i: any) => ({ foodItemId: i.foodItemId, name: i.itemName, qty: i.quantity, addOnNotes: i.addOnNotes ?? null })),
      readyItems: nonDelivered
        .filter((i: any) => i.isKitchenReady)
        .map((i: any) => ({ foodItemId: i.foodItemId, name: i.itemName, qty: i.quantity, addOnNotes: i.addOnNotes ?? null })),
      elapsedMs: Date.now() - new Date(o.createdDate ?? o.orderDate).getTime(),
      marking:   false,
    };
  }

  private tickElapsed(): void {
    this.orders.update(list =>
      list.map(o => ({ ...o, elapsedMs: Date.now() - new Date(o.createdAt).getTime() }))
    );
  }

  elapsedLabel(ms: number): string {
    const m = Math.floor(ms / 60000);
    if (m < 1)  return 'just now';
    if (m < 60) return `${m}m ago`;
    return `${Math.floor(m / 60)}h ${m % 60}m ago`;
  }

  isLate(ms: number):    boolean { return ms > 15 * 60 * 1000; }
  isWarning(ms: number): boolean { return ms > 8  * 60 * 1000 && ms <= 15 * 60 * 1000; }

  formatTime(date: Date | string): string {
    return new Date(date).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit' });
  }
}

import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTableModule } from '@angular/material/table';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { interval, Subscription, switchMap } from 'rxjs';
import { OrderService } from '../../../core/services/order.service';
import { TableService } from '../../../core/services/table.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { WhatsAppService } from '../../../core/services/whatsapp.service';
import { AuthService } from '../../../core/authentication/auth.service';
import { APP_CONSTANTS } from '../../../core/constants/app.constants';
import { HasPermissionDirective } from '../../../core/directives/has-permission.directive';
import { BillDialogComponent, BillDialogResult } from '../pos/bill-dialog.component';
import { PaymentQrDialogComponent, PaymentQrDialogData } from '../payment-qr-dialog/payment-qr-dialog.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { ConfirmationDialogComponent } from '../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { TransferTableDialogComponent, TransferDialogResult } from '../transfer-table-dialog/transfer-table-dialog.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { Order } from '../../../core/models/order.model';
import { Table } from '../../../core/models/table.model';

type StatusFilter = 'All' | 'Pending' | 'KitchenReady' | 'BillPending' | 'Completed' | 'Cancelled';
type TypeFilter   = 'All' | 'DineIn' | 'Takeaway';
type ViewMode     = 'list' | 'card';
const CANCEL_WINDOW_MS = 60_000;

@Component({
  selector: 'app-orders-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatChipsModule,
    MatTooltipModule,
    MatTableModule,
    MatExpansionModule,
    MatProgressSpinnerModule,
    HasPermissionDirective,
    LoadingSpinnerComponent,
    EmptyStateComponent,
  ],
  templateUrl: './orders-list.component.html',
  styleUrl: './orders-list.component.scss',
})
export class OrdersListComponent implements OnInit, OnDestroy {
  private orderService = inject(OrderService);
  private tableService = inject(TableService);
  private whatsapp     = inject(WhatsAppService);
  private signalR      = inject(SignalRService);
  private authService  = inject(AuthService);
  private dialog       = inject(MatDialog);
  private notify       = inject(NotificationService);

  get isWaiter(): boolean { return this.authService.userRole() === APP_CONSTANTS.ROLES.WAITER; }
  get isAdmin():  boolean { return this.authService.userRole() === APP_CONSTANTS.ROLES.ADMIN; }

  orders       = signal<Order[]>([]);
  tables       = signal<Table[]>([]);
  loading      = signal(false);
  searchTerm   = signal('');
  statusFilter = signal<StatusFilter>('All');
  typeFilter   = signal<TypeFilter>('All');
  historyMode  = signal(false);
  viewMode     = signal<ViewMode>('list');

  cancelling   = signal<number | null>(null);
  transferring = signal<number | null>(null);
  completing   = signal<number | null>(null);
  reopening    = signal<number | null>(null);
  deleting     = signal<number | null>(null);

  private tick       = signal(0);
  private tickSub!:   Subscription;
  private signalSubs: Subscription[] = [];

  readonly statusOptions: StatusFilter[] = ['All', 'Pending', 'KitchenReady', 'BillPending', 'Completed', 'Cancelled'];

  filtered = computed(() => {
    let list       = this.orders();
    const sf       = this.statusFilter();
    const tf       = this.typeFilter();
    const q        = this.searchTerm().toLowerCase().trim();
    const hist     = this.historyMode();

    if (hist) {
      list = list.filter(o => o.status === 'Completed' || o.status === 'Cancelled');
    } else if (sf !== 'All') {
      list = list.filter(o => o.status === sf);
    }

    if (tf === 'DineIn')   list = list.filter(o => o.tableId !== null);
    if (tf === 'Takeaway') list = list.filter(o => o.tableId === null);

    if (q) list = list.filter(o =>
      o.orderNumber.toLowerCase().includes(q) ||
      o.cashierName.toLowerCase().includes(q)
    );
    return list;
  });

  countByStatus(status: StatusFilter): number {
    if (status === 'All') return this.orders().length;
    return this.orders().filter(o => o.status === status).length;
  }

  ngOnInit(): void {
    this.load();
    this.tableService.getAll().subscribe(res => this.tables.set((res.data ?? []).filter(t => t.isActive)));
    this.tickSub = interval(1000).subscribe(() => this.tick.set(this.tick() + 1));
    this.signalSubs.push(
      this.signalR.orderRejectedByKitchen$.subscribe(ev => {
        const loc = ev.tableNumber ? `Table ${ev.tableNumber}` : 'Takeaway';
        const reason = ev.reason ? ` — ${ev.reason}` : '';
        this.notify.error(`⚠️ Order ${ev.orderNumber} (${loc}) REJECTED by kitchen${reason}`);
        this.load();
      }),
      this.signalR.orderStatusChanged$.subscribe(() => this.load()),
    );
  }

  ngOnDestroy(): void {
    this.tickSub?.unsubscribe();
    this.signalSubs.forEach(s => s.unsubscribe());
  }

  load(): void {
    this.loading.set(true);
    this.orderService.getAll().subscribe({
      next:  res => { this.orders.set(res.data ?? []); this.loading.set(false); },
      error: ()  => { this.notify.error('Failed to load orders.'); this.loading.set(false); },
    });
  }

  // ── Time helpers ──────────────────────────────────────────────────────────
  cancelSecondsLeft(order: Order): number {
    this.tick();
    const rem = Math.floor((new Date(order.createdDate).getTime() + CANCEL_WINDOW_MS - Date.now()) / 1000);
    return Math.max(0, rem);
  }
  canCancel(order: Order): boolean {
    return order.status !== 'Cancelled' && this.cancelSecondsLeft(order) > 0;
  }

  // ── Cancel ────────────────────────────────────────────────────────────────
  cancelOrder(order: Order): void {
    const ref = this.dialog.open(ConfirmationDialogComponent, {
      data: { title: 'Cancel Order', message: `Cancel ${order.orderNumber}? Stock will be restored.`, confirmText: 'Cancel Order', danger: true },
      width: '420px',
    });
    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.cancelling.set(order.id);
      this.orderService.cancel(order.id).subscribe({
        next: () => {
          this.notify.success(`Order ${order.orderNumber} cancelled. Stock restored.`);
          this.cancelling.set(null);
          if (!order.tableId && (order as any).customerPhone) {
            this.whatsapp.sendOrderCancellation((order as any).customerPhone, {
              orderNumber: order.orderNumber, grandTotal: order.grandTotal,
            });
          }
          this.load();
        },
        error: err => { this.notify.error(err?.error?.message || 'Failed to cancel order.'); this.cancelling.set(null); },
      });
    });
  }

  // ── Reopen ────────────────────────────────────────────────────────────────
  reopenOrder(order: Order): void {
    const ref = this.dialog.open(ConfirmationDialogComponent, {
      data: { title: 'Reopen Order', message: `Reopen ${order.orderNumber}? It will be restored to Pending and stock re-deducted.`, confirmText: 'Reopen', danger: false },
      width: '420px',
    });
    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.reopening.set(order.id);
      this.orderService.reopen(order.id).subscribe({
        next: () => {
          this.notify.success(`Order ${order.orderNumber} reopened.`);
          this.reopening.set(null);
          this.load();
        },
        error: err => { this.notify.error(err?.error?.message || 'Failed to reopen order.'); this.reopening.set(null); },
      });
    });
  }

  // ── Delete ────────────────────────────────────────────────────────────────
  deleteOrder(order: Order): void {
    const ref = this.dialog.open(ConfirmationDialogComponent, {
      data: { title: 'Delete Order', message: `Permanently delete ${order.orderNumber}? This cannot be undone.`, confirmText: 'Delete', danger: true },
      width: '420px',
    });
    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.deleting.set(order.id);
      this.orderService.delete(order.id).subscribe({
        next: () => {
          this.notify.success(`Order ${order.orderNumber} deleted.`);
          this.deleting.set(null);
          this.load();
        },
        error: err => { this.notify.error(err?.error?.message || 'Failed to delete order.'); this.deleting.set(null); },
      });
    });
  }

  // ── Complete (generate-bill → confirm-payment) ────────────────────────────
  completeOrder(order: Order): void {
    const ref = this.dialog.open(ConfirmationDialogComponent, {
      data: { title: 'Complete Order', message: `Mark ${order.orderNumber} as completed without editing the bill? Current total ₹${order.grandTotal.toFixed(2)}.`, confirmText: 'Complete', danger: false },
      width: '420px',
    });
    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.completing.set(order.id);
      this.orderService.generateBill(order.id, order.discount).pipe(
        switchMap(() => this.orderService.confirmPayment(order.id))
      ).subscribe({
        next: () => {
          this.notify.success(`Order ${order.orderNumber} completed.`);
          this.completing.set(null);
          this.load();
        },
        error: err => { this.notify.error(err?.error?.message || 'Failed to complete order.'); this.completing.set(null); },
      });
    });
  }

  // ── Transfer ──────────────────────────────────────────────────────────────
  openTransfer(order: Order): void {
    const ref = this.dialog.open(TransferTableDialogComponent, {
      data: { orderNumber: order.orderNumber, currentTableId: order.tableId ?? null, tables: this.tables() },
      width: '460px',
    });
    ref.afterClosed().subscribe((result: TransferDialogResult | null) => {
      if (!result) return;
      this.transferring.set(order.id);
      const call = result.newTableId !== null
        ? this.orderService.transferTable(order.id, result.newTableId)
        : this.orderService.transferToTakeaway(order.id);
      call.subscribe({
        next: () => {
          const label = result.newTableId !== null
            ? `Table ${this.tables().find(t => t.id === result.newTableId)?.tableNumber}`
            : 'Takeaway';
          this.notify.success(`Order ${order.orderNumber} moved to ${label}.`);
          this.transferring.set(null);
          this.load();
        },
        error: err => { this.notify.error(err?.error?.message || 'Transfer failed.'); this.transferring.set(null); },
      });
    });
  }

  // ── Generate Bill ─────────────────────────────────────────────────────────
  generateBill(order: Order): void {
    const ref = this.dialog.open(BillDialogComponent, {
      data: { orderNumber: order.orderNumber, tableNumber: order.tableNumber, subTotal: order.subTotal, tax: order.tax, currentDiscount: order.discount },
      width: '420px',
    });
    ref.afterClosed().subscribe((result: BillDialogResult | null) => {
      if (result === null || result === undefined) return;
      this.orderService.generateBill(order.id, result.discount).subscribe({
        next: res => {
          if (res.success) {
            this.notify.success(`Bill generated for ${order.orderNumber}.`);
            this.orderService.openReceipt(res.data);
            const payRef = this.dialog.open(PaymentQrDialogComponent, {
              data: { orderId: res.data.orderId, orderNumber: res.data.orderNumber, grandTotal: res.data.grandTotal, upiId: res.data.upiId, restaurantName: res.data.restaurantName } as PaymentQrDialogData,
              width: '400px',
            });
            payRef.afterClosed().subscribe(() => this.load());
          } else {
            this.notify.error(res.message || 'Failed to generate bill.');
          }
        },
        error: err => this.notify.error(err?.error?.message || 'Failed to generate bill.'),
      });
    });
  }

  viewReceipt(order: Order): void { this.orderService.openReceipt(order); }

  // ── History toggle ────────────────────────────────────────────────────────
  toggleHistory(): void {
    const next = !this.historyMode();
    this.historyMode.set(next);
    if (next) this.statusFilter.set('All');
  }

  // ── Helpers ───────────────────────────────────────────────────────────────
  getStatusClass(status: string): string { return status.toLowerCase().replace(/\s+/g, ''); }
  getStatusLabel(status: string): string { return status === 'KitchenReady' ? 'Served' : status; }

  getStatusIcon(status: string): string {
    switch (status) {
      case 'Completed':    return 'check_circle';
      case 'Cancelled':    return 'cancel';
      case 'KitchenReady': return 'room_service';
      case 'BillPending':  return 'payment';
      default:             return 'hourglass_empty';
    }
  }

  firstItemName(order: Order): string {
    const first = order.items?.[0]?.itemName ?? 'Order';
    return order.items?.length > 1 ? `${first} +${order.items.length - 1}` : first;
  }
}

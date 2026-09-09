import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { forkJoin, Subscription } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSelectModule } from '@angular/material/select';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDialog } from '@angular/material/dialog';
import { TableService } from '../../core/services/table.service';
import { OrderService } from '../../core/services/order.service';
import { AuthService } from '../../core/authentication/auth.service';
import { NotificationService } from '../../shared/services/notification.service';
import { ConfirmationDialogComponent } from '../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { QrDialogComponent, QrDialogData } from './qr-dialog.component';
import { ReservationDialogComponent } from './reservation-dialog.component';
import { TableOrderViewDialogComponent } from './table-order-view-dialog.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { Table, HallType, HALL_OPTIONS } from '../../core/models/table.model';
import { Order } from '../../core/models/order.model';
import { SignalRService } from '../../core/services/signalr.service';
import { APP_CONSTANTS } from '../../core/constants/app.constants';

interface StatusConfig {
  label: string;
  icon:  string;
  color: string;
  bg:    string;
  pulse: boolean;
}

@Component({
  selector: 'app-tables',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatCardModule, MatButtonModule, MatIconModule,
    MatInputModule, MatFormFieldModule, MatTooltipModule, MatSlideToggleModule,
    MatBadgeModule, MatSelectModule,
    LoadingSpinnerComponent, EmptyStateComponent,
  ],
  templateUrl: './tables.component.html',
  styleUrl: './tables.component.scss',
})
export class TablesComponent implements OnInit, OnDestroy {
  private tableService = inject(TableService);
  private orderService = inject(OrderService);
  private signalR      = inject(SignalRService);
  private authService  = inject(AuthService);
  private notify       = inject(NotificationService);
  private dialog       = inject(MatDialog);
  private fb           = inject(FormBuilder);

  private subs: Subscription[] = [];

  get isAdmin(): boolean { return this.authService.userRole() === APP_CONSTANTS.ROLES.ADMIN; }

  readonly hallOptions = HALL_OPTIONS;

  tables        = signal<Table[]>([]);
  pendingOrders = signal<Order[]>([]);
  loading       = signal(false);
  saving        = signal(false);
  editingId     = signal<number | null>(null);

  form = this.fb.group({
    tableNumber: [null as number | null, [Validators.required, Validators.min(1)]],
    capacity:    [null as number | null, [Validators.required, Validators.min(1)]],
    hall:        ['AC' as HallType,      Validators.required],
  });

  editForm = this.fb.group({
    tableNumber: [null as number | null, [Validators.required, Validators.min(1)]],
    capacity:    [null as number | null, [Validators.required, Validators.min(1)]],
    hall:        ['AC' as HallType,      Validators.required],
    isActive:    [true],
  });

  readonly occupied  = computed(() => this.tables().filter(t => t.isOccupied).length);
  readonly available = computed(() => this.tables().filter(t => t.isActive && !t.isOccupied).length);
  readonly total     = computed(() => this.tables().filter(t => t.isActive).length);

  readonly latestUpdates = computed(() =>
    [...this.pendingOrders()]
      .filter(o => o.tableNumber != null)
      .sort((a, b) => new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime())
      .slice(0, 4)
  );

  readonly groupedTables = computed(() => {
    const groups: Record<string, Table[]> = {};
    for (const t of this.tables()) {
      const key = t.hall ?? 'AC';
      if (!groups[key]) groups[key] = [];
      groups[key].push(t);
    }
    return groups;
  });

  getTableStatusClass(table: Table): string {
    if (!table.isActive) return 'tbl-inactive';
    if (table.billPending) return 'tbl-bill-pending';
    if (table.isOccupied) return 'tbl-on-dine';
    return 'tbl-available';
  }

  getChairDist(capacity: number): { top: number; bottom: number; left: number; right: number } {
    const c = Math.max(1, capacity || 2);
    if (c <= 2) return { top: 1, bottom: 1, left: 0, right: 0 };

    // Short sides (left/right) get 1–3 chairs depending on table size
    const perSide = c <= 4 ? 1 : c <= 14 ? 2 : 3;
    const remaining = c - perSide * 2;
    const top = Math.ceil(remaining / 2);
    const bottom = remaining - top;
    return { top, bottom, left: perSide, right: perSide };
  }

  getTableBoxStyle(capacity: number): { [key: string]: string } {
    const dist  = this.getChairDist(capacity);
    const CSLOT = 23;  // chair width (17) + gap (6)
    const VSLOT = 26;  // chair height (21) + gap (5)
    const w = Math.max(100, dist.top    * CSLOT - 6 + 24);
    const h = Math.max(64,  dist.left   * VSLOT - 5 + 24);
    return { width: w + 'px', height: h + 'px' };
  }

  range(n: number): number[] {
    return n > 0 ? Array.from({ length: n }, (_, i) => i) : [];
  }

  ngOnInit(): void {
    this.load();
    this.subs.push(
      this.signalR.orderCreated$.subscribe(()       => this.loadFeed()),
      this.signalR.orderStatusChanged$.subscribe(() => this.loadFeed()),
    );
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
  }

  private loadFeed(): void {
    forkJoin({
      tables: this.tableService.getAll(),
      orders: this.orderService.getPending(),
    }).subscribe({
      next: ({ tables, orders }) => {
        this.tables.set(tables.data ?? []);
        this.pendingOrders.set(orders.data ?? []);
      },
    });
  }

  load(): void {
    this.loading.set(true);
    forkJoin({
      tables: this.tableService.getAll(),
      orders: this.orderService.getPending(),
    }).subscribe({
      next: ({ tables, orders }) => {
        this.tables.set(tables.data ?? []);
        this.pendingOrders.set(orders.data ?? []);
        this.loading.set(false);
      },
      error: () => { this.notify.error('Failed to load tables.'); this.loading.set(false); },
    });
  }

  getStatusConfig(status: string): StatusConfig {
    switch (status) {
      case 'Pending':      return { label: 'New Order',      icon: 'receipt_long',  color: '#1565c0', bg: '#e3f2fd', pulse: true  };
      case 'KitchenReady': return { label: 'Ready to Serve', icon: 'room_service',  color: '#1b5e20', bg: '#e8f5e9', pulse: true  };
      case 'BillPending':  return { label: 'Bill Pending',   icon: 'payment',       color: '#880e4f', bg: '#fce4ec', pulse: false };
      default:             return { label: status,           icon: 'info_outline',  color: '#616161', bg: '#f5f5f5', pulse: false };
    }
  }

  addTable(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    const v = this.form.getRawValue();
    this.tableService.create({ tableNumber: v.tableNumber!, capacity: v.capacity!, hall: v.hall! as HallType }).subscribe({
      next: () => {
        this.notify.success('Table added successfully.');
        this.form.reset();
        this.saving.set(false);
        this.load();
      },
      error: (err) => {
        this.notify.error(err?.error?.message || 'Failed to add table.');
        this.saving.set(false);
      },
    });
  }

  getEditingTable(): Table | null {
    const id = this.editingId();
    return id !== null ? (this.tables().find(t => t.id === id) ?? null) : null;
  }

  startEdit(table: Table): void {
    this.editingId.set(table.id);
    this.editForm.patchValue({ tableNumber: table.tableNumber, capacity: table.capacity, hall: table.hall, isActive: table.isActive });
  }

  cancelEdit(): void { this.editingId.set(null); }

  saveEditById(): void {
    const table = this.getEditingTable();
    if (table) this.saveEdit(table);
  }

  saveEdit(table: Table): void {
    if (this.editForm.invalid) return;
    this.saving.set(true);
    const v = this.editForm.getRawValue();
    this.tableService.update(table.id, { tableNumber: v.tableNumber!, capacity: v.capacity!, hall: v.hall! as HallType, isActive: v.isActive! }).subscribe({
      next: () => {
        this.notify.success('Table updated.');
        this.editingId.set(null);
        this.saving.set(false);
        this.load();
      },
      error: (err) => {
        this.notify.error(err?.error?.message || 'Failed to update table.');
        this.saving.set(false);
      },
    });
  }

  openReservations(): void {
    this.dialog.open(ReservationDialogComponent, {
      data: { tables: this.tables().filter(t => t.isActive) },
      width: '1000px',
      maxWidth: '98vw',
      height: '85vh',
      panelClass: 'reservation-dialog-panel',
    });
  }

  showQr(table: Table): void {
    const origin = window.location.origin;
    const menuUrl = `${origin}/menu?table=${table.tableNumber}`;
    this.dialog.open(QrDialogComponent, {
      data: { tableNumber: table.tableNumber, menuUrl } as QrDialogData,
      width: '380px',
    });
  }

  viewOrder(table: Table, event: Event): void {
    event.stopPropagation();
    this.orderService.getPending().subscribe({
      next: (res) => {
        const order = (res.data ?? []).find(o => o.tableId === table.id);
        if (!order) { this.notify.error('No active order found for this table.'); return; }
        const ref = this.dialog.open(TableOrderViewDialogComponent, {
          data: { table, order, onPaid: () => this.load() },
          width: '540px',
          maxHeight: '90vh',
        });
        ref.afterClosed().subscribe((r) => { if (r?.paid) this.load(); });
      },
      error: () => this.notify.error('Could not load order details.'),
    });
  }

  deleteTable(table: Table): void {
    const ref = this.dialog.open(ConfirmationDialogComponent, {
      data: { title: 'Delete Table', message: `Remove Table ${table.tableNumber}?`, confirmText: 'Delete', danger: true },
      width: '400px',
    });
    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.tableService.delete(table.id).subscribe({
        next: () => { this.notify.success('Table deleted.'); this.load(); },
        error: (err) => this.notify.error(err?.error?.message || 'Failed to delete table.'),
      });
    });
  }
}

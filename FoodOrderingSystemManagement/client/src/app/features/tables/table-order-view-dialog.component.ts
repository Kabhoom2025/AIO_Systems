import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Order, OrderItem } from '../../core/models/order.model';
import { Table } from '../../core/models/table.model';
import { OrderService } from '../../core/services/order.service';
import { NotificationService } from '../../shared/services/notification.service';

export interface TableOrderViewData {
  table: Table;
  order: Order;
  onPaid?: () => void;
}

type ItemStatus = 'ordered' | 'in-kitchen' | 'ready' | 'served';

@Component({
  selector: 'app-table-order-view-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  template: `
    <div class="tov-wrap">

      <!-- Header -->
      <div class="tov-header">
        <div class="tov-header-left">
          <mat-icon>visibility</mat-icon>
          <div>
            <span class="tov-title">Table #{{ data.table.tableNumber }}</span>
            <span class="tov-sub">{{ data.table.hall }} Hall &nbsp;·&nbsp; {{ data.order.orderNumber }}</span>
          </div>
        </div>
        <button mat-icon-button (click)="close()" class="tov-close"><mat-icon>close</mat-icon></button>
      </div>

      <!-- Order meta row -->
      <div class="tov-meta">
        <div class="tov-meta-item">
          <mat-icon>schedule</mat-icon>
          <span>{{ data.order.orderDate | date:'hh:mm a' }}</span>
        </div>
        <div class="tov-meta-item">
          <mat-icon>person</mat-icon>
          <span>{{ data.order.cashierName }}</span>
        </div>
        <div class="tov-meta-item">
          <mat-icon>receipt</mat-icon>
          <span>₹{{ data.order.grandTotal | number:'1.2-2' }}</span>
        </div>
        <div class="tov-status-chip" [class]="'chip-' + data.order.status.toLowerCase().replace(' ', '-')">
          {{ data.order.status }}
        </div>
      </div>

      <!-- Bill Paid confirmation banner (shows for 10s after Paid clicked) -->
      @if (paidBanner()) {
        <div class="tov-paid-banner">
          <mat-icon>check_circle</mat-icon>
          <span>Bill Paid — Table cleared!</span>
        </div>
      }

      <!-- Payment Pending banner -->
      @if (data.order.status === 'BillPending' && !paidBanner()) {
        <div class="tov-payment-banner">
          <div class="tov-payment-banner-left">
            <mat-icon>payment</mat-icon>
            <span>Payment Pending</span>
          </div>
          <button mat-raised-button class="tov-paid-btn" (click)="confirmPaid()" [disabled]="confirming()">
            @if (confirming()) {
              <mat-spinner diameter="14" strokeWidth="2"></mat-spinner>
              &nbsp;Confirming…
            } @else {
              <ng-container>
                <mat-icon>check_circle</mat-icon>
                Paid
              </ng-container>
            }
          </button>
        </div>
      }

      <!-- Status summary bar -->
      <div class="tov-summary">
        @for (s of statusSummary; track s.label) {
          <div class="tov-summary-item">
            <span class="tov-summary-dot" [class]="'dot-' + s.key"></span>
            <span class="tov-summary-count">{{ s.count }}</span>
            <span class="tov-summary-label">{{ s.label }}</span>
          </div>
        }
      </div>

      <!-- Items list -->
      <div class="tov-items">
        @for (item of data.order.items; track item.foodItemId) {
          <div class="tov-item" [class]="'item-' + getItemStatus(item)">
            <div class="tov-item-left">
              <div class="tov-item-status-bar" [class]="'bar-' + getItemStatus(item)"></div>
              <div class="tov-item-info">
                <span class="tov-item-name">{{ item.itemName }}</span>
                @if (item.addOnNotes) {
                  <span class="tov-item-addons">+ {{ item.addOnNotes }}</span>
                }
              </div>
            </div>
            <div class="tov-item-right">
              <span class="tov-item-qty">× {{ item.quantity }}</span>
              <span class="tov-item-price">₹{{ item.lineTotal | number:'1.2-2' }}</span>
              <span class="tov-item-badge" [class]="'badge-' + getItemStatus(item)">
                <mat-icon>{{ statusIcon(item) }}</mat-icon>
                {{ statusLabel(item) }}
              </span>
            </div>
          </div>
        }
      </div>

      <!-- Footer -->
      <div class="tov-footer">
        <button mat-stroked-button (click)="close()">Close</button>
      </div>

    </div>
  `,
  styles: [`
    .tov-wrap { width: 520px; font-family: inherit; display: flex; flex-direction: column; }

    /* Header */
    .tov-header {
      display: flex; align-items: center; justify-content: space-between;
      background: #ff5722; color: #fff; padding: 14px 16px 14px 20px;
    }
    .tov-header-left { display: flex; align-items: center; gap: 12px; mat-icon { font-size: 22px; } }
    .tov-title { display: block; font-size: 1rem; font-weight: 700; line-height: 1.2; }
    .tov-sub   { display: block; font-size: .72rem; opacity: .8; margin-top: 1px; }
    .tov-close { color: #fff !important; }

    /* Meta row */
    .tov-meta {
      display: flex; align-items: center; gap: 16px; flex-wrap: wrap;
      padding: 10px 20px; background: #fff8f6; border-bottom: 1px solid #ffe0d6;
    }
    .tov-meta-item {
      display: flex; align-items: center; gap: 4px;
      font-size: .78rem; color: #5d4037;
      mat-icon { font-size: 15px; width: 15px; height: 15px; color: #ff5722; }
    }
    .tov-status-chip {
      margin-left: auto; font-size: .7rem; font-weight: 700; letter-spacing: .06em;
      text-transform: uppercase; padding: 3px 10px; border-radius: 20px;
      &.chip-pending      { background: #fff3e0; color: #e65100; }
      &.chip-kitchenready { background: #e8f5e9; color: #2e7d32; }
      &.chip-completed    { background: #e3f2fd; color: #1565c0; }
      &.chip-billpending  { background: #fce4ec; color: #880e4f; }
    }

    /* Payment pending banner */
    .tov-payment-banner {
      display: flex; align-items: center; justify-content: space-between;
      padding: 10px 16px; background: #fce4ec; border-bottom: 1px solid #f48fb1;
    }
    .tov-payment-banner-left {
      display: flex; align-items: center; gap: 8px;
      font-size: .83rem; font-weight: 700; color: #880e4f;
      mat-icon { font-size: 18px; width: 18px; height: 18px; }
    }
    .tov-paid-btn {
      background: #2e7d32 !important; color: #fff !important;
      font-size: .78rem; height: 32px; display: flex; align-items: center; gap: 4px;
      mat-icon { font-size: 16px; width: 16px; height: 16px; }
    }

    /* Bill paid success banner */
    .tov-paid-banner {
      display: flex; align-items: center; justify-content: center; gap: 8px;
      padding: 12px 16px; background: #e8f5e9;
      font-size: .88rem; font-weight: 700; color: #1b5e20;
      mat-icon { color: #2e7d32; font-size: 20px; width: 20px; height: 20px; }
    }

    /* Summary bar */
    .tov-summary {
      display: flex; gap: 0; border-bottom: 1px solid #f5f5f5;
    }
    .tov-summary-item {
      flex: 1; display: flex; align-items: center; justify-content: center; gap: 6px;
      padding: 10px 8px; font-size: .75rem; border-right: 1px solid #f5f5f5;
      &:last-child { border-right: none; }
    }
    .tov-summary-dot {
      width: 8px; height: 8px; border-radius: 50%; flex-shrink: 0;
      &.dot-ordered    { background: #90a4ae; }
      &.dot-in-kitchen { background: #ffb74d; }
      &.dot-ready      { background: #66bb6a; }
      &.dot-served     { background: #42a5f5; }
    }
    .tov-summary-count { font-weight: 700; font-size: .9rem; color: #212121; }
    .tov-summary-label { color: #757575; }

    /* Items */
    .tov-items {
      max-height: 340px; overflow-y: auto;
      display: flex; flex-direction: column;
    }
    .tov-item {
      display: flex; align-items: center; justify-content: space-between;
      padding: 10px 16px 10px 0; border-bottom: 1px solid #fafafa;
      gap: 8px;
      &:last-child { border-bottom: none; }
    }
    .tov-item-left  { display: flex; align-items: center; gap: 0; flex: 1; min-width: 0; }
    .tov-item-right { display: flex; align-items: center; gap: 10px; flex-shrink: 0; }

    .tov-item-status-bar {
      width: 3px; min-height: 38px; border-radius: 2px; margin-right: 12px; flex-shrink: 0;
      &.bar-ordered    { background: #90a4ae; }
      &.bar-in-kitchen { background: #ffb74d; }
      &.bar-ready      { background: #66bb6a; }
      &.bar-served     { background: #42a5f5; }
    }

    .tov-item-info { display: flex; flex-direction: column; gap: 2px; min-width: 0; }
    .tov-item-name   { font-size: .87rem; font-weight: 600; color: #212121; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .tov-item-addons { font-size: .72rem; color: #9e9e9e; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

    .tov-item-qty   { font-size: .78rem; color: #9e9e9e; min-width: 28px; text-align: right; }
    .tov-item-price { font-size: .82rem; font-weight: 600; color: #424242; min-width: 64px; text-align: right; font-variant-numeric: tabular-nums; }

    .tov-item-badge {
      display: flex; align-items: center; gap: 3px;
      font-size: .68rem; font-weight: 700; letter-spacing: .04em;
      text-transform: uppercase; padding: 3px 8px; border-radius: 20px; white-space: nowrap;
      mat-icon { font-size: 13px; width: 13px; height: 13px; }
      &.badge-ordered    { background: #eceff1; color: #546e7a; }
      &.badge-in-kitchen { background: #fff8e1; color: #f57f17; }
      &.badge-ready      { background: #e8f5e9; color: #2e7d32; }
      &.badge-served     { background: #e3f2fd; color: #1565c0; }
    }

    /* Footer */
    .tov-footer {
      display: flex; justify-content: flex-end;
      padding: 12px 16px; border-top: 1px solid #f0f0f0;
    }
  `],
})
export class TableOrderViewDialogComponent {
  private dialogRef    = inject(MatDialogRef<TableOrderViewDialogComponent>);
  readonly data        = inject<TableOrderViewData>(MAT_DIALOG_DATA);
  private orderService = inject(OrderService);
  private notify       = inject(NotificationService);

  confirming = signal(false);
  paidBanner = signal(false);

  get statusSummary() {
    const counts = { ordered: 0, 'in-kitchen': 0, ready: 0, served: 0 };
    this.data.order.items.forEach(i => counts[this.getItemStatus(i)]++);
    return [
      { key: 'ordered',    label: 'Ordered',    count: counts['ordered'] },
      { key: 'in-kitchen', label: 'In Kitchen',  count: counts['in-kitchen'] },
      { key: 'ready',      label: 'Ready',       count: counts['ready'] },
      { key: 'served',     label: 'Served',      count: counts['served'] },
    ];
  }

  getItemStatus(item: OrderItem): ItemStatus {
    if (item.isDelivered)    return 'served';
    if (item.isKitchenReady) return 'ready';
    return (this.data.order.status === 'Pending' || this.data.order.status === 'BillPending') ? 'ordered' : 'in-kitchen';
  }

  statusLabel(item: OrderItem): string {
    const map: Record<ItemStatus, string> = {
      'ordered':    'Ordered',
      'in-kitchen': 'In Kitchen',
      'ready':      'Ready',
      'served':     'Served',
    };
    return map[this.getItemStatus(item)];
  }

  statusIcon(item: OrderItem): string {
    const map: Record<ItemStatus, string> = {
      'ordered':    'fiber_new',
      'in-kitchen': 'soup_kitchen',
      'ready':      'check_circle',
      'served':     'done_all',
    };
    return map[this.getItemStatus(item)];
  }

  confirmPaid(): void {
    this.confirming.set(true);
    this.orderService.confirmPayment(this.data.order.id).subscribe({
      next: () => {
        this.confirming.set(false);
        this.paidBanner.set(true);
        this.data.onPaid?.();
        setTimeout(() => {
          this.paidBanner.set(false);
          this.dialogRef.close({ paid: true });
        }, 10000);
      },
      error: (err) => {
        this.confirming.set(false);
        this.notify.error(err?.error?.message || 'Failed to confirm payment.');
      },
    });
  }

  close(): void { this.dialogRef.close(); }
}

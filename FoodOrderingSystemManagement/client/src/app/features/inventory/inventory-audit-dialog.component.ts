import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { InventoryService } from '../../core/services/inventory.service';
import { InventoryItem, StockTransaction } from '../../core/models/inventory.model';

@Component({
  selector: 'app-inventory-audit-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatTooltipModule,
  ],
  template: `
    <h2 mat-dialog-title class="dlg-title">
      <mat-icon>history</mat-icon>
      Audit Trail — {{ item.name }}
    </h2>

    <mat-dialog-content class="dlg-body">
      @if (loading()) {
        <div class="center-state"><mat-spinner diameter="32" /></div>
      } @else if (transactions().length === 0) {
        <div class="empty-state">
          <mat-icon>receipt_long</mat-icon>
          <p>No transactions recorded yet.</p>
        </div>
      } @else {
        <div class="tx-list">
          @for (tx of transactions(); track tx.id) {
            <div class="tx-row" [class]="'tx-' + txClass(tx.transactionType)">
              <div class="tx-icon-wrap">
                <mat-icon class="tx-icon">{{ txIcon(tx.transactionType) }}</mat-icon>
              </div>
              <div class="tx-main">
                <div class="tx-header">
                  <span class="tx-type-badge" [class]="'badge-' + txClass(tx.transactionType)">
                    {{ tx.transactionType }}
                  </span>
                  <span class="tx-qty" [class.plus]="tx.quantity > 0" [class.minus]="tx.quantity < 0">
                    {{ tx.quantity > 0 ? '+' : '' }}{{ tx.quantity | number:'1.0-3' }} {{ item.unit }}
                  </span>
                </div>
                <div class="tx-stock">
                  {{ tx.stockBefore | number:'1.0-3' }} → {{ tx.stockAfter | number:'1.0-3' }} {{ item.unit }}
                </div>
                @if (tx.supplier) {
                  <div class="tx-meta"><mat-icon>storefront</mat-icon> {{ tx.supplier }}</div>
                }
                @if (tx.referenceNumber) {
                  <div class="tx-meta"><mat-icon>receipt</mat-icon> {{ tx.referenceNumber }}</div>
                }
                @if (tx.batchNumber) {
                  <div class="tx-meta"><mat-icon>inventory</mat-icon> Batch: {{ tx.batchNumber }}</div>
                }
                @if (tx.expiryDate) {
                  <div class="tx-meta" [class.expiry-warn]="isExpired(tx.expiryDate)">
                    <mat-icon>event</mat-icon> Expires: {{ tx.expiryDate | date:'dd MMM yyyy' }}
                    @if (isExpired(tx.expiryDate)) { <span class="exp-badge">EXPIRED</span> }
                  </div>
                }
                @if (tx.notes) {
                  <div class="tx-notes">{{ tx.notes }}</div>
                }
              </div>
              <div class="tx-date">{{ tx.createdAt | date:'dd MMM yy, HH:mm' }}</div>
            </div>
          }
        </div>
      }
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close()">Close</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dlg-title { display:flex; align-items:center; gap:8px; font-size:1rem; font-weight:700; }
    .dlg-body { min-width:480px; max-width:620px; max-height:60vh; overflow-y:auto; padding:0 4px; }
    .center-state, .empty-state { display:flex; flex-direction:column; align-items:center;
      justify-content:center; gap:8px; padding:32px; color:#888; }
    .empty-state mat-icon { font-size:40px; width:40px; height:40px; }

    .tx-list { display:flex; flex-direction:column; gap:8px; }
    .tx-row { display:flex; gap:12px; background:#fafafa; border-radius:8px; padding:10px 12px;
      border-left:4px solid #ccc; }
    .tx-purchase { border-color:#4caf50; background:#f1f8e9; }
    .tx-waste { border-color:#f44336; background:#fff8f8; }
    .tx-transfer { border-color:#9c27b0; background:#f3e5f5; }
    .tx-adjustment { border-color:#2196f3; background:#e3f2fd; }
    .tx-opening { border-color:#ff9800; background:#fff8e1; }

    .tx-icon-wrap { display:flex; align-items:flex-start; padding-top:2px; }
    .tx-icon { font-size:20px; width:20px; height:20px; }
    .tx-purchase .tx-icon { color:#388e3c; }
    .tx-waste .tx-icon { color:#d32f2f; }
    .tx-transfer .tx-icon { color:#7b1fa2; }
    .tx-adjustment .tx-icon { color:#1565c0; }
    .tx-opening .tx-icon { color:#e65100; }

    .tx-main { flex:1; display:flex; flex-direction:column; gap:3px; }
    .tx-header { display:flex; align-items:center; gap:8px; flex-wrap:wrap; }
    .tx-type-badge { font-size:.68rem; font-weight:700; padding:1px 7px; border-radius:10px;
      text-transform:uppercase; letter-spacing:.5px; }
    .badge-purchase { background:#c8e6c9; color:#1b5e20; }
    .badge-waste { background:#ffcdd2; color:#b71c1c; }
    .badge-transfer { background:#e1bee7; color:#4a148c; }
    .badge-adjustment { background:#bbdefb; color:#0d47a1; }
    .badge-opening { background:#ffe0b2; color:#bf360c; }

    .tx-qty { font-weight:700; font-size:.85rem; margin-left:auto; }
    .tx-qty.plus { color:#388e3c; }
    .tx-qty.minus { color:#d32f2f; }

    .tx-stock { font-size:.75rem; color:#666; }
    .tx-meta { display:flex; align-items:center; gap:4px; font-size:.73rem; color:#555; }
    .tx-meta mat-icon { font-size:13px; width:13px; height:13px; }
    .tx-notes { font-size:.75rem; color:#777; font-style:italic; margin-top:2px; }
    .exp-badge { font-size:.65rem; font-weight:700; padding:0 5px; border-radius:4px;
      background:#ffcdd2; color:#b71c1c; margin-left:4px; }
    .expiry-warn { color:#c62828; }
    .tx-date { font-size:.72rem; color:#999; white-space:nowrap; padding-top:2px; }

    mat-dialog-actions { padding:12px 24px; }
  `],
})
export class InventoryAuditDialogComponent implements OnInit {
  dialogRef = inject(MatDialogRef<InventoryAuditDialogComponent>);
  item: InventoryItem = inject(MAT_DIALOG_DATA);
  private inventoryService = inject(InventoryService);

  transactions = signal<StockTransaction[]>([]);
  loading = signal(true);

  ngOnInit(): void {
    this.inventoryService.getTransactions(this.item.id).subscribe({
      next: res => { this.transactions.set(res.data ?? []); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  txIcon(type: string): string {
    switch (type.toLowerCase()) {
      case 'purchase':   return 'shopping_cart';
      case 'waste':      return 'delete_forever';
      case 'transfer':   return 'swap_horiz';
      case 'adjustment': return 'tune';
      case 'opening':    return 'inventory';
      default:           return 'receipt_long';
    }
  }

  txClass(type: string): string {
    return type.toLowerCase();
  }

  isExpired(dateStr: string): boolean {
    return new Date(dateStr) < new Date();
  }
}

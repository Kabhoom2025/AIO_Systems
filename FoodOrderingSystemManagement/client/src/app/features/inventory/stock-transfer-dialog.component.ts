import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { InventoryItem, TransferStockRequest } from '../../core/models/inventory.model';

export interface StockTransferDialogData {
  sourceItem: InventoryItem;
  allItems: InventoryItem[];
}

@Component({
  selector: 'app-stock-transfer-dialog',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatDialogModule, MatButtonModule, MatIconModule,
    MatInputModule, MatFormFieldModule, MatSelectModule,
  ],
  template: `
    <h2 mat-dialog-title class="dlg-title">
      <mat-icon>swap_horiz</mat-icon>
      Stock Transfer
    </h2>

    <mat-dialog-content class="dlg-body">
      <div class="transfer-route">
        <div class="route-item source">
          <mat-icon>inventory_2</mat-icon>
          <span class="route-name">{{ data.sourceItem.name }}</span>
          <span class="route-stock">{{ data.sourceItem.currentStock }} {{ data.sourceItem.unit }}</span>
        </div>
        <mat-icon class="arrow">east</mat-icon>
        <div class="route-item target">
          <mat-icon>inventory_2</mat-icon>
          <span class="route-name">{{ targetName }}</span>
        </div>
      </div>

      <div class="form-grid">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Transfer To *</mat-label>
          <mat-select [(ngModel)]="form.targetInventoryItemId">
            @for (item of otherItems; track item.id) {
              <mat-option [value]="item.id">
                {{ item.name }} ({{ item.currentStock }} {{ item.unit }})
              </mat-option>
            }
          </mat-select>
          <mat-icon matPrefix>move_down</mat-icon>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Quantity to Transfer *</mat-label>
          <input matInput type="number" [(ngModel)]="form.quantity" min="0.01" step="0.01"
                 [max]="data.sourceItem.currentStock" required />
          <span matSuffix>{{ data.sourceItem.unit }}</span>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Notes</mat-label>
          <textarea matInput [(ngModel)]="form.notes" rows="2" maxlength="300"
                    placeholder="Optional reason for transfer"></textarea>
        </mat-form-field>
      </div>

      @if (form.quantity > 0 && form.targetInventoryItemId) {
        <div class="preview">
          <mat-icon>info_outline</mat-icon>
          <strong>{{ data.sourceItem.name }}</strong>: {{ data.sourceItem.currentStock }}
          → {{ data.sourceItem.currentStock - form.quantity | number:'1.0-3' }} {{ data.sourceItem.unit }}
        </div>
      }
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close()">Cancel</button>
      <button mat-flat-button color="accent"
              [disabled]="!form.targetInventoryItemId || !form.quantity || form.quantity <= 0 || form.quantity > data.sourceItem.currentStock"
              (click)="submit()">
        <mat-icon>swap_horiz</mat-icon>
        Transfer
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dlg-title { display:flex; align-items:center; gap:8px; font-size:1rem; font-weight:700; }
    .dlg-body { min-width:400px; max-width:520px; }
    .transfer-route { display:flex; align-items:center; gap:8px; background:#f3e5f5;
      border-radius:8px; padding:10px 14px; margin-bottom:14px; }
    .route-item { display:flex; align-items:center; gap:6px; flex:1; }
    .route-name { font-weight:600; font-size:.85rem; }
    .route-stock { font-size:.75rem; color:#666; }
    .source mat-icon, .target mat-icon { color:#7b1fa2; font-size:18px; }
    .arrow { color:#7b1fa2; }
    .form-grid { display:grid; grid-template-columns:1fr; gap:8px; }
    .full-width { grid-column:1/-1; }
    mat-form-field { width:100%; }
    .preview { display:flex; align-items:center; gap:6px; font-size:.82rem;
      background:#e8eaf6; border-radius:8px; padding:8px 14px; margin-top:4px; color:#283593; }
    mat-dialog-actions { padding:12px 24px; }
  `],
})
export class StockTransferDialogComponent {
  dialogRef = inject(MatDialogRef<StockTransferDialogComponent>);
  data: StockTransferDialogData = inject(MAT_DIALOG_DATA);

  form: TransferStockRequest = {
    targetInventoryItemId: 0,
    quantity: 0,
    notes: '',
  };

  get otherItems(): InventoryItem[] {
    return this.data.allItems.filter(i => i.id !== this.data.sourceItem.id && i.isActive);
  }

  get targetName(): string {
    const t = this.data.allItems.find(i => i.id === this.form.targetInventoryItemId);
    return t ? t.name : '—';
  }

  submit(): void {
    if (!this.form.targetInventoryItemId || !this.form.quantity || this.form.quantity <= 0) return;
    const payload: TransferStockRequest = {
      targetInventoryItemId: this.form.targetInventoryItemId,
      quantity: this.form.quantity,
      notes: this.form.notes || undefined,
    };
    this.dialogRef.close(payload);
  }
}

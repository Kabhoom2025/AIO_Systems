import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { InventoryItem, PurchaseStockRequest } from '../../core/models/inventory.model';

@Component({
  selector: 'app-purchase-stock-dialog',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatDialogModule, MatButtonModule, MatIconModule,
    MatInputModule, MatFormFieldModule,
  ],
  template: `
    <h2 mat-dialog-title class="dlg-title">
      <mat-icon>shopping_cart</mat-icon>
      Purchase Stock — {{ item.name }}
    </h2>

    <mat-dialog-content class="dlg-body">
      <div class="current-info">
        <span class="ci-label">Current stock</span>
        <span class="ci-value">{{ item.currentStock }} {{ item.unit }}</span>
      </div>

      <div class="form-grid">
        <mat-form-field appearance="outline">
          <mat-label>Quantity Received *</mat-label>
          <input matInput type="number" [(ngModel)]="form.quantity" min="0.01" step="0.01" required />
          <span matSuffix>{{ item.unit }}</span>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Supplier Name</mat-label>
          <input matInput [(ngModel)]="form.supplier" placeholder="e.g. FreshFarm Co." maxlength="150" />
          <mat-icon matPrefix>storefront</mat-icon>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Invoice / PO Number</mat-label>
          <input matInput [(ngModel)]="form.referenceNumber" placeholder="e.g. INV-20260101" maxlength="100" />
          <mat-icon matPrefix>receipt</mat-icon>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Batch Number</mat-label>
          <input matInput [(ngModel)]="form.batchNumber" placeholder="e.g. BATCH-001" maxlength="50" />
          <mat-icon matPrefix>inventory</mat-icon>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Expiry Date</mat-label>
          <input matInput type="date" [(ngModel)]="form.expiryDate" />
          <mat-icon matPrefix>event</mat-icon>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Notes</mat-label>
          <textarea matInput [(ngModel)]="form.notes" rows="2" maxlength="300"
                    placeholder="Optional notes about this purchase"></textarea>
        </mat-form-field>
      </div>

      @if (form.quantity > 0) {
        <div class="preview">
          <mat-icon>trending_up</mat-icon>
          New stock will be <strong>{{ item.currentStock + form.quantity }} {{ item.unit }}</strong>
        </div>
      }
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close()">Cancel</button>
      <button mat-flat-button color="primary"
              [disabled]="!form.quantity || form.quantity <= 0"
              (click)="submit()">
        <mat-icon>add_circle</mat-icon>
        Record Purchase
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dlg-title { display:flex; align-items:center; gap:8px; font-size:1rem; font-weight:700; }
    .dlg-body { min-width:400px; max-width:520px; }
    .current-info { display:flex; justify-content:space-between; align-items:center;
      background:#e8f5e9; border-radius:8px; padding:8px 14px; margin-bottom:14px; font-size:.85rem; }
    .ci-label { color:#388e3c; font-weight:600; }
    .ci-value { font-weight:700; color:#1b5e20; }
    .form-grid { display:grid; grid-template-columns:1fr 1fr; gap:8px 12px; }
    .full-width { grid-column:1/-1; }
    mat-form-field { width:100%; }
    .preview { display:flex; align-items:center; gap:6px; font-size:.82rem;
      background:#e3f2fd; border-radius:8px; padding:8px 14px; margin-top:4px; color:#1565c0; }
    mat-dialog-actions { padding:12px 24px; }
  `],
})
export class PurchaseStockDialogComponent {
  dialogRef = inject(MatDialogRef<PurchaseStockDialogComponent>);
  item: InventoryItem = inject(MAT_DIALOG_DATA);

  form: PurchaseStockRequest = {
    quantity: 0,
    supplier: '',
    referenceNumber: '',
    batchNumber: '',
    expiryDate: '',
    notes: '',
  };

  submit(): void {
    if (!this.form.quantity || this.form.quantity <= 0) return;
    const payload: PurchaseStockRequest = {
      quantity: this.form.quantity,
      supplier: this.form.supplier || undefined,
      referenceNumber: this.form.referenceNumber || undefined,
      batchNumber: this.form.batchNumber || undefined,
      expiryDate: this.form.expiryDate || undefined,
      notes: this.form.notes || undefined,
    };
    this.dialogRef.close(payload);
  }
}

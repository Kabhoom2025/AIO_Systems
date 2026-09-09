import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { InventoryItem, WasteStockRequest } from '../../core/models/inventory.model';

const WASTE_REASONS = [
  'Spoilage / Expired',
  'Contamination',
  'Breakage / Spillage',
  'Overproduction',
  'Pest damage',
  'Quality failure',
  'Other',
];

@Component({
  selector: 'app-waste-management-dialog',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatDialogModule, MatButtonModule, MatIconModule,
    MatInputModule, MatFormFieldModule, MatSelectModule,
  ],
  template: `
    <h2 mat-dialog-title class="dlg-title">
      <mat-icon>delete_forever</mat-icon>
      Record Waste — {{ item.name }}
    </h2>

    <mat-dialog-content class="dlg-body">
      <div class="current-info">
        <span class="ci-label">Current stock</span>
        <span class="ci-value">{{ item.currentStock }} {{ item.unit }}</span>
      </div>

      <div class="form-grid">
        <mat-form-field appearance="outline">
          <mat-label>Waste Quantity *</mat-label>
          <input matInput type="number" [(ngModel)]="form.quantity" min="0.01" step="0.01" required />
          <span matSuffix>{{ item.unit }}</span>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Waste Reason</mat-label>
          <mat-select [(ngModel)]="form.wasteReason">
            @for (r of wasteReasons; track r) {
              <mat-option [value]="r">{{ r }}</mat-option>
            }
          </mat-select>
          <mat-icon matPrefix>report_problem</mat-icon>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Batch Number</mat-label>
          <input matInput [(ngModel)]="form.batchNumber" placeholder="e.g. BATCH-001" maxlength="50" />
          <mat-icon matPrefix>inventory</mat-icon>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Additional Notes</mat-label>
          <textarea matInput [(ngModel)]="form.notes" rows="2" maxlength="300"
                    placeholder="Optional details about this waste entry"></textarea>
        </mat-form-field>
      </div>

      @if (form.quantity > 0) {
        <div class="preview" [class.warn]="newStock < item.minimumStock">
          <mat-icon>{{ newStock < item.minimumStock ? 'warning' : 'remove_circle_outline' }}</mat-icon>
          New stock will be <strong>{{ newStock | number:'1.0-3' }} {{ item.unit }}</strong>
          @if (newStock < item.minimumStock) {
            <span class="warn-text">— below minimum threshold!</span>
          }
        </div>
      }
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close()">Cancel</button>
      <button mat-flat-button color="warn"
              [disabled]="!form.quantity || form.quantity <= 0"
              (click)="submit()">
        <mat-icon>delete_sweep</mat-icon>
        Record Waste
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dlg-title { display:flex; align-items:center; gap:8px; font-size:1rem; font-weight:700; }
    .dlg-body { min-width:400px; max-width:520px; }
    .current-info { display:flex; justify-content:space-between; align-items:center;
      background:#fff3e0; border-radius:8px; padding:8px 14px; margin-bottom:14px; font-size:.85rem; }
    .ci-label { color:#e65100; font-weight:600; }
    .ci-value { font-weight:700; color:#bf360c; }
    .form-grid { display:grid; grid-template-columns:1fr 1fr; gap:8px 12px; }
    .full-width { grid-column:1/-1; }
    mat-form-field { width:100%; }
    .preview { display:flex; align-items:center; gap:6px; font-size:.82rem;
      background:#fce4ec; border-radius:8px; padding:8px 14px; margin-top:4px; color:#c62828; }
    .preview.warn { background:#ffebee; }
    .warn-text { color:#b71c1c; font-weight:600; }
    mat-dialog-actions { padding:12px 24px; }
  `],
})
export class WasteManagementDialogComponent {
  dialogRef = inject(MatDialogRef<WasteManagementDialogComponent>);
  item: InventoryItem = inject(MAT_DIALOG_DATA);

  wasteReasons = WASTE_REASONS;

  form: WasteStockRequest = {
    quantity: 0,
    wasteReason: '',
    batchNumber: '',
    notes: '',
  };

  get newStock(): number {
    return Math.max(0, this.item.currentStock - (this.form.quantity || 0));
  }

  submit(): void {
    if (!this.form.quantity || this.form.quantity <= 0) return;
    const payload: WasteStockRequest = {
      quantity: this.form.quantity,
      wasteReason: this.form.wasteReason || undefined,
      batchNumber: this.form.batchNumber || undefined,
      notes: this.form.notes || undefined,
    };
    this.dialogRef.close(payload);
  }
}

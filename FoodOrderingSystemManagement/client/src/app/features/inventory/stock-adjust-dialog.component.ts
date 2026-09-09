import { Component, inject, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { InventoryItem } from '../../core/models/inventory.model';

export interface StockAdjustDialogData {
  item: InventoryItem;
  fromScan?: boolean;
}

@Component({
  selector: 'app-stock-adjust-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
  ],
  template: `
    <div class="sad-wrap">
      <div class="sad-header">
        <mat-icon>{{ data.fromScan ? 'qr_code_scanner' : 'tune' }}</mat-icon>
        <span class="sad-header-title">Adjust Stock — {{ data.item.name }}</span>
        @if (data.fromScan) {
          <span class="sad-scan-badge">Scanned</span>
        }
      </div>

      <div class="sad-current">
        <span class="sad-lbl">Current stock</span>
        <span class="sad-val">{{ data.item.currentStock }} {{ data.item.unit }}</span>
      </div>

      <form [formGroup]="form" class="sad-body" (ngSubmit)="submit()">

        <div class="sad-quick">
          <button type="button" class="quick-btn add" (click)="setQty(10)">+10</button>
          <button type="button" class="quick-btn add" (click)="setQty(50)">+50</button>
          <button type="button" class="quick-btn remove" (click)="setQty(-10)">−10</button>
          <button type="button" class="quick-btn remove" (click)="setQty(-50)">−50</button>
        </div>

        <mat-form-field appearance="outline">
          <mat-label>Quantity (positive = add, negative = remove)</mat-label>
          <input matInput type="number" formControlName="quantity" step="0.5" #quantityInput />
          @if (form.get('quantity')!.hasError('required') && form.get('quantity')!.touched) {
            <mat-error>Quantity is required.</mat-error>
          }
          @if (form.get('quantity')!.hasError('nonZero') && form.get('quantity')!.touched) {
            <mat-error>Quantity cannot be zero.</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Note (optional)</mat-label>
          <input matInput formControlName="note" placeholder="e.g. Delivery received, wastage…" />
        </mat-form-field>

        @if (preview !== null) {
          <div class="sad-preview" [class.low]="preview <= data.item.minimumStock">
            <mat-icon>{{ preview <= data.item.minimumStock ? 'warning_amber' : 'check_circle' }}</mat-icon>
            New stock: <strong>{{ preview | number:'1.0-2' }} {{ data.item.unit }}</strong>
          </div>
        }

        <div class="sad-actions">
          <button type="button" class="sad-btn-cancel" (click)="cancel()">Cancel</button>
          <button type="submit" class="sad-btn-save" [disabled]="form.invalid">
            <mat-icon>check</mat-icon>
            Apply
          </button>
        </div>

      </form>
    </div>
  `,
  styles: [`
    .sad-wrap { width: 400px; font-family: inherit; }
    .sad-header {
      display: flex; align-items: center; gap: 10px;
      background: var(--primary); color: #fff;
      padding: 16px 20px; font-size: 1rem; font-weight: 700;
      mat-icon { font-size: 22px; }
    }
    .sad-header-title { flex: 1; }
    .sad-scan-badge {
      font-size: .7rem; font-weight: 700; letter-spacing: .06em;
      background: rgba(255,255,255,.22); border: 1px solid rgba(255,255,255,.35);
      border-radius: 6px; padding: 3px 8px; white-space: nowrap;
    }
    .sad-current {
      display: flex; justify-content: space-between; align-items: center;
      padding: 12px 20px; background: #f9f9f9; border-bottom: 1px solid #f0f0f0;
      .sad-lbl { font-size: .8rem; color: #9e9e9e; }
      .sad-val { font-weight: 700; font-size: 1rem; color: #212121; }
    }
    .sad-body { padding: 16px 20px; display: flex; flex-direction: column; gap: 10px; }
    mat-form-field { width: 100%; }
    .sad-quick {
      display: flex; gap: 8px; flex-wrap: wrap;
      .quick-btn {
        border-radius: 8px; border: 1.5px solid; padding: 5px 14px;
        cursor: pointer; font-size: .85rem; font-weight: 600; transition: all .12s;
        &.add    { border-color: #4caf50; color: #2e7d32; background: #f1f8e9; &:hover { background: #c8e6c9; } }
        &.remove { border-color: #ef5350; color: #c62828; background: #ffebee; &:hover { background: #ffcdd2; } }
      }
    }
    .sad-preview {
      display: flex; align-items: center; gap: 8px;
      padding: 10px 12px; border-radius: 8px;
      background: #e8f5e9; color: #2e7d32; font-size: .87rem;
      mat-icon { font-size: 18px; }
      &.low { background: #fff8e1; color: #e65100; }
    }
    .sad-actions {
      display: flex; gap: 10px; justify-content: flex-end;
      padding-top: 10px; border-top: 1px solid #f0f0f0; margin-top: 4px;
    }
    .sad-btn-cancel {
      background: none; border: 1px solid #ddd; border-radius: 8px;
      padding: 0 18px; height: 38px; cursor: pointer; font-size: .87rem; color: #666;
      &:hover { background: #f5f5f5; }
    }
    .sad-btn-save {
      display: flex; align-items: center; gap: 6px;
      background: var(--primary); color: #fff; border: none; border-radius: 8px;
      padding: 0 20px; height: 38px; font-size: .87rem; font-weight: 600; cursor: pointer;
      mat-icon { font-size: 18px; }
      &:hover { opacity: .9; }
      &:disabled { opacity: .5; cursor: not-allowed; }
    }
  `],
})
export class StockAdjustDialogComponent implements OnInit, AfterViewInit {
  private fb        = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<StockAdjustDialogComponent>);
  readonly data     = inject<StockAdjustDialogData>(MAT_DIALOG_DATA);

  @ViewChild('quantityInput') quantityInput!: ElementRef<HTMLInputElement>;

  form!: FormGroup;

  get preview(): number | null {
    const qty = this.form?.get('quantity')?.value;
    if (qty === null || qty === '' || isNaN(qty) || qty === 0) return null;
    return Math.max(0, this.data.item.currentStock + Number(qty));
  }

  ngOnInit(): void {
    this.form = this.fb.group({
      quantity: [null, [Validators.required, this.nonZeroValidator]],
      note:     [''],
    });
  }

  ngAfterViewInit(): void {
    if (this.data.fromScan) {
      setTimeout(() => this.quantityInput?.nativeElement?.focus(), 150);
    }
  }

  private nonZeroValidator(control: { value: number }) {
    return control.value === 0 ? { nonZero: true } : null;
  }

  setQty(value: number): void {
    this.form.patchValue({ quantity: value });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.dialogRef.close(this.form.value);
  }

  cancel(): void { this.dialogRef.close(); }
}

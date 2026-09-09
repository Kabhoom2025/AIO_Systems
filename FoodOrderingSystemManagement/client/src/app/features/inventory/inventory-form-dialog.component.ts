import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSelectModule } from '@angular/material/select';
import { InventoryItem } from '../../core/models/inventory.model';

export interface InventoryFormDialogData {
  item?: InventoryItem;
}

const COMMON_UNITS = ['kg', 'grams', 'liters', 'ml', 'pieces', 'packets', 'boxes', 'bottles', 'dozen'];

@Component({
  selector: 'app-inventory-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSlideToggleModule,
    MatSelectModule,
  ],
  template: `
    <div class="ifd-wrap">
      <div class="ifd-header">
        <mat-icon>{{ isEdit ? 'edit' : 'add_box' }}</mat-icon>
        <span>{{ isEdit ? 'Edit Inventory Item' : 'New Inventory Item' }}</span>
      </div>

      <form [formGroup]="form" class="ifd-body" (ngSubmit)="submit()">

        <mat-form-field appearance="outline">
          <mat-label>Name</mat-label>
          <input matInput formControlName="name" placeholder="e.g. Tomatoes" />
          @if (form.get('name')!.hasError('required') && form.get('name')!.touched) {
            <mat-error>Name is required.</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Description (optional)</mat-label>
          <input matInput formControlName="description" placeholder="Brief note…" />
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Barcode (optional)</mat-label>
          <input matInput formControlName="barcode" placeholder="e.g. CAPSICUM or 4001234567" />
          <mat-icon matPrefix>qr_code_scanner</mat-icon>
          <mat-hint>Scan this value with the camera to locate the item</mat-hint>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Unit</mat-label>
          <mat-select formControlName="unit">
            @for (u of units; track u) {
              <mat-option [value]="u">{{ u }}</mat-option>
            }
          </mat-select>
          @if (form.get('unit')!.hasError('required') && form.get('unit')!.touched) {
            <mat-error>Unit is required.</mat-error>
          }
        </mat-form-field>

        <div class="ifd-row">
          <mat-form-field appearance="outline">
            <mat-label>Current Stock</mat-label>
            <input matInput type="number" formControlName="currentStock" min="0" step="0.5" />
            @if (form.get('currentStock')!.hasError('min') && form.get('currentStock')!.touched) {
              <mat-error>Cannot be negative.</mat-error>
            }
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Minimum Stock</mat-label>
            <input matInput type="number" formControlName="minimumStock" min="0" step="0.5" />
            <mat-hint>Alert threshold</mat-hint>
            @if (form.get('minimumStock')!.hasError('min') && form.get('minimumStock')!.touched) {
              <mat-error>Cannot be negative.</mat-error>
            }
          </mat-form-field>
        </div>

        <mat-form-field appearance="outline">
          <mat-label>Expiry Date (optional)</mat-label>
          <input matInput type="date" formControlName="expiryDate" />
          <mat-icon matPrefix>event</mat-icon>
          <mat-hint>Used for expiry alerts</mat-hint>
        </mat-form-field>

        @if (isEdit) {
          <div class="ifd-toggle">
            <mat-slide-toggle formControlName="isActive" color="primary">Active</mat-slide-toggle>
            <span class="ifd-toggle-hint">Inactive items are excluded from alerts.</span>
          </div>
        }

        <div class="ifd-actions">
          <button type="button" class="ifd-btn-cancel" (click)="cancel()">Cancel</button>
          <button type="submit" class="ifd-btn-save" [disabled]="form.invalid">
            <mat-icon>{{ isEdit ? 'save' : 'add' }}</mat-icon>
            {{ isEdit ? 'Save Changes' : 'Create' }}
          </button>
        </div>

      </form>
    </div>
  `,
  styles: [`
    .ifd-wrap { width: 480px; font-family: inherit; }
    .ifd-header {
      display: flex; align-items: center; gap: 10px;
      background: var(--primary); color: #fff;
      padding: 16px 20px; font-size: 1rem; font-weight: 700;
      mat-icon { font-size: 22px; }
    }
    .ifd-body { padding: 20px; display: flex; flex-direction: column; gap: 8px; }
    mat-form-field { width: 100%; }
    .ifd-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    .ifd-toggle { display: flex; align-items: center; gap: 14px; padding: 8px 0; }
    .ifd-toggle-hint { font-size: .75rem; color: #9e9e9e; }
    .ifd-actions {
      display: flex; gap: 10px; justify-content: flex-end;
      padding-top: 12px; border-top: 1px solid #f0f0f0; margin-top: 4px;
    }
    .ifd-btn-cancel {
      background: none; border: 1px solid #ddd; border-radius: 8px;
      padding: 0 18px; height: 38px; cursor: pointer; font-size: .87rem; color: #666;
      &:hover { background: #f5f5f5; }
    }
    .ifd-btn-save {
      display: flex; align-items: center; gap: 6px;
      background: var(--primary); color: #fff; border: none; border-radius: 8px;
      padding: 0 20px; height: 38px; font-size: .87rem; font-weight: 600; cursor: pointer;
      mat-icon { font-size: 18px; }
      &:hover { opacity: .9; }
      &:disabled { opacity: .5; cursor: not-allowed; }
    }
  `],
})
export class InventoryFormDialogComponent implements OnInit {
  private fb        = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<InventoryFormDialogComponent>);
  readonly data     = inject<InventoryFormDialogData>(MAT_DIALOG_DATA);

  form!: FormGroup;
  isEdit = false;
  units = COMMON_UNITS;

  ngOnInit(): void {
    this.isEdit = !!this.data?.item;
    const item = this.data?.item;

    this.form = this.fb.group({
      name:         [item?.name         ?? '', [Validators.required, Validators.maxLength(100)]],
      description:  [item?.description  ?? ''],
      barcode:      [item?.barcode      ?? ''],
      unit:         [item?.unit         ?? '', Validators.required],
      currentStock: [item?.currentStock ?? 0,  [Validators.required, Validators.min(0)]],
      minimumStock: [item?.minimumStock ?? 0,  [Validators.required, Validators.min(0)]],
      expiryDate:   [item?.expiryDate ? item.expiryDate.substring(0, 10) : ''],
      ...(this.isEdit ? { isActive: [item?.isActive ?? true] } : {}),
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.dialogRef.close(this.form.value);
  }

  cancel(): void { this.dialogRef.close(); }
}

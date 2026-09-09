import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { Supplier, PurchaseOrder, PAYMENT_METHODS } from '../../core/models/supplier.model';

export interface SupplierPaymentDialogData {
  supplier: Supplier;
  purchaseOrders: PurchaseOrder[];
}

@Component({
  selector: 'app-supplier-payment-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
            MatInputModule, MatButtonModule, MatIconModule, MatSelectModule],
  template: `
    <div class="spd-wrap">
      <div class="spd-header">
        <mat-icon>payments</mat-icon>
        <div>
          <span class="spd-title">Record Payment</span>
          <span class="spd-sub">{{ data.supplier.name }}</span>
        </div>
      </div>
      <form [formGroup]="form" class="spd-body" (ngSubmit)="submit()">
        <mat-form-field appearance="outline">
          <mat-label>Link to Purchase Order (optional)</mat-label>
          <mat-select formControlName="purchaseOrderId">
            <mat-option [value]="null">— None —</mat-option>
            @for (po of data.purchaseOrders; track po.id) {
              <mat-option [value]="po.id">{{ po.poNumber }} — ₹{{ po.totalAmount | number:'1.2-2' }} ({{ po.status }})</mat-option>
            }
          </mat-select>
        </mat-form-field>
        <div class="spd-row">
          <mat-form-field appearance="outline">
            <mat-label>Amount *</mat-label>
            <input matInput type="number" formControlName="amount" min="0.01" step="0.01" />
            <span matPrefix>₹</span>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Payment Method *</mat-label>
            <mat-select formControlName="paymentMethod">
              @for (m of methods; track m) {
                <mat-option [value]="m">{{ m }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
        </div>
        <div class="spd-row">
          <mat-form-field appearance="outline">
            <mat-label>Payment Date *</mat-label>
            <input matInput type="date" formControlName="paymentDate" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Reference / Cheque No.</mat-label>
            <input matInput formControlName="referenceNumber" />
          </mat-form-field>
        </div>
        <mat-form-field appearance="outline">
          <mat-label>Notes</mat-label>
          <textarea matInput formControlName="notes" rows="2"></textarea>
        </mat-form-field>
        <div class="spd-actions">
          <button type="button" class="btn-cancel" (click)="cancel()">Cancel</button>
          <button type="submit" class="btn-save" [disabled]="form.invalid">
            <mat-icon>payments</mat-icon> Record Payment
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .spd-wrap { width: 500px; font-family: inherit; }
    .spd-header { display: flex; align-items: center; gap: 12px; background: var(--primary); color: #fff; padding: 16px 20px; mat-icon { font-size: 24px; } }
    .spd-title { display: block; font-size: 1rem; font-weight: 700; }
    .spd-sub { display: block; font-size: .78rem; opacity: .8; }
    .spd-body { padding: 20px; display: flex; flex-direction: column; gap: 10px; }
    mat-form-field { width: 100%; }
    .spd-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    .spd-actions { display: flex; gap: 10px; justify-content: flex-end; padding-top: 10px; border-top: 1px solid #f0f0f0; }
    .btn-cancel { background: none; border: 1px solid #ddd; border-radius: 8px; padding: 0 18px; height: 38px; cursor: pointer; font-size: .87rem; color: #666; &:hover { background: #f5f5f5; } }
    .btn-save { display: flex; align-items: center; gap: 6px; background: var(--primary); color: #fff; border: none; border-radius: 8px; padding: 0 20px; height: 38px; font-size: .87rem; font-weight: 600; cursor: pointer; &:hover { opacity: .9; } &:disabled { opacity: .5; cursor: not-allowed; } mat-icon { font-size: 18px; } }
  `],
})
export class SupplierPaymentDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<SupplierPaymentDialogComponent>);
  readonly data = inject<SupplierPaymentDialogData>(MAT_DIALOG_DATA);

  form!: FormGroup;
  methods = PAYMENT_METHODS;

  ngOnInit(): void {
    const today = new Date().toISOString().substring(0, 10);
    this.form = this.fb.group({
      purchaseOrderId: [null],
      amount: [null, [Validators.required, Validators.min(0.01)]],
      paymentMethod: ['Cash', Validators.required],
      paymentDate: [today, Validators.required],
      referenceNumber: [''],
      notes: [''],
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.dialogRef.close({ supplierId: this.data.supplier.id, ...this.form.value });
  }
  cancel(): void { this.dialogRef.close(); }
}

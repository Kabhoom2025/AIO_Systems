import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { Supplier } from '../../core/models/supplier.model';

export interface SupplierFormDialogData { supplier?: Supplier; }

@Component({
  selector: 'app-supplier-form-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
            MatInputModule, MatButtonModule, MatIconModule, MatSlideToggleModule],
  template: `
    <div class="sfd-wrap">
      <div class="sfd-header">
        <mat-icon>{{ isEdit ? 'edit' : 'add_business' }}</mat-icon>
        <span>{{ isEdit ? 'Edit Supplier' : 'New Supplier' }}</span>
      </div>
      <form [formGroup]="form" class="sfd-body" (ngSubmit)="submit()">
        <div class="sfd-row">
          <mat-form-field appearance="outline">
            <mat-label>Supplier Name *</mat-label>
            <input matInput formControlName="name" />
            @if (form.get('name')!.hasError('required') && form.get('name')!.touched) {
              <mat-error>Name is required.</mat-error>
            }
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Contact Person</mat-label>
            <input matInput formControlName="contactPerson" />
          </mat-form-field>
        </div>
        <div class="sfd-row">
          <mat-form-field appearance="outline">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" />
            <mat-icon matPrefix>email</mat-icon>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Phone</mat-label>
            <input matInput formControlName="phone" />
            <mat-icon matPrefix>phone</mat-icon>
          </mat-form-field>
        </div>
        <mat-form-field appearance="outline">
          <mat-label>Address</mat-label>
          <textarea matInput formControlName="address" rows="2"></textarea>
        </mat-form-field>
        <div class="sfd-row">
          <mat-form-field appearance="outline">
            <mat-label>Tax / GST Number</mat-label>
            <input matInput formControlName="taxNumber" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Payment Terms</mat-label>
            <input matInput formControlName="paymentTerms" placeholder="e.g. Net 30" />
          </mat-form-field>
        </div>
        <mat-form-field appearance="outline">
          <mat-label>Notes</mat-label>
          <textarea matInput formControlName="notes" rows="2"></textarea>
        </mat-form-field>
        @if (isEdit) {
          <div class="sfd-toggle">
            <mat-slide-toggle formControlName="isActive" color="primary">Active</mat-slide-toggle>
          </div>
        }
        <div class="sfd-actions">
          <button type="button" class="btn-cancel" (click)="cancel()">Cancel</button>
          <button type="submit" class="btn-save" [disabled]="form.invalid">
            <mat-icon>{{ isEdit ? 'save' : 'add' }}</mat-icon>
            {{ isEdit ? 'Save Changes' : 'Create Supplier' }}
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .sfd-wrap { width: 560px; font-family: inherit; }
    .sfd-header { display: flex; align-items: center; gap: 10px; background: var(--primary); color: #fff; padding: 16px 20px; font-size: 1rem; font-weight: 700; mat-icon { font-size: 22px; } }
    .sfd-body { padding: 20px; display: flex; flex-direction: column; gap: 8px; }
    .sfd-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    mat-form-field { width: 100%; }
    .sfd-toggle { padding: 6px 0; }
    .sfd-actions { display: flex; gap: 10px; justify-content: flex-end; padding-top: 12px; border-top: 1px solid #f0f0f0; }
    .btn-cancel { background: none; border: 1px solid #ddd; border-radius: 8px; padding: 0 18px; height: 38px; cursor: pointer; font-size: .87rem; color: #666; &:hover { background: #f5f5f5; } }
    .btn-save { display: flex; align-items: center; gap: 6px; background: var(--primary); color: #fff; border: none; border-radius: 8px; padding: 0 20px; height: 38px; font-size: .87rem; font-weight: 600; cursor: pointer; mat-icon { font-size: 18px; } &:hover { opacity: .9; } &:disabled { opacity: .5; cursor: not-allowed; } }
  `],
})
export class SupplierFormDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<SupplierFormDialogComponent>);
  readonly data = inject<SupplierFormDialogData>(MAT_DIALOG_DATA);

  form!: FormGroup;
  isEdit = false;

  ngOnInit(): void {
    this.isEdit = !!this.data?.supplier;
    const s = this.data?.supplier;
    this.form = this.fb.group({
      name: [s?.name ?? '', [Validators.required, Validators.maxLength(200)]],
      contactPerson: [s?.contactPerson ?? ''],
      email: [s?.email ?? '', Validators.email],
      phone: [s?.phone ?? ''],
      address: [s?.address ?? ''],
      taxNumber: [s?.taxNumber ?? ''],
      paymentTerms: [s?.paymentTerms ?? ''],
      notes: [s?.notes ?? ''],
      ...(this.isEdit ? { isActive: [s?.isActive ?? true] } : {}),
    });
  }

  submit(): void { if (this.form.invalid) { this.form.markAllAsTouched(); return; } this.dialogRef.close(this.form.value); }
  cancel(): void { this.dialogRef.close(); }
}

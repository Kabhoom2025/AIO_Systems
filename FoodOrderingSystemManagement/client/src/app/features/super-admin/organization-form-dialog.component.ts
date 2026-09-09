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
import { Organization } from '../../core/models/organization.model';

export interface OrgFormDialogData { organization?: Organization; }

@Component({
  selector: 'app-organization-form-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
            MatInputModule, MatButtonModule, MatIconModule, MatSlideToggleModule, MatSelectModule],
  template: `
    <div class="ofd-wrap">
      <div class="ofd-header">
        <mat-icon>{{ isEdit ? 'edit' : 'add_business' }}</mat-icon>
        <span>{{ isEdit ? 'Edit Organization' : 'Add Organization' }}</span>
      </div>
      <form [formGroup]="form" class="ofd-body" (ngSubmit)="submit()">
        <div class="ofd-row">
          <mat-form-field appearance="outline">
            <mat-label>Organization Name *</mat-label>
            <input matInput formControlName="name" />
            @if (form.get('name')!.hasError('required') && form.get('name')!.touched) {
              <mat-error>Name is required.</mat-error>
            }
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" />
            <mat-icon matPrefix>email</mat-icon>
          </mat-form-field>
        </div>
        <div class="ofd-row">
          <mat-form-field appearance="outline">
            <mat-label>Phone</mat-label>
            <input matInput formControlName="phone" />
            <mat-icon matPrefix>phone</mat-icon>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Currency</mat-label>
            <mat-select formControlName="currency">
              <mat-option value="INR">INR (₹)</mat-option>
              <mat-option value="USD">USD ($)</mat-option>
              <mat-option value="EUR">EUR (€)</mat-option>
            </mat-select>
          </mat-form-field>
        </div>
        <mat-form-field appearance="outline">
          <mat-label>Address</mat-label>
          <textarea matInput formControlName="address" rows="2"></textarea>
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Timezone</mat-label>
          <input matInput formControlName="timezone" placeholder="e.g. Asia/Kolkata" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Logo URL (optional)</mat-label>
          <input matInput formControlName="logoUrl" />
        </mat-form-field>
        @if (isEdit) {
          <div class="ofd-toggle">
            <mat-slide-toggle formControlName="isActive" color="primary">Active</mat-slide-toggle>
          </div>
        }
        <div class="ofd-actions">
          <button type="button" class="btn-cancel" (click)="cancel()">Cancel</button>
          <button type="submit" class="btn-save" [disabled]="form.invalid">
            <mat-icon>{{ isEdit ? 'save' : 'add' }}</mat-icon>
            {{ isEdit ? 'Save Changes' : 'Add Organization' }}
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .ofd-wrap { width: 500px; font-family: inherit; }
    .ofd-header { display: flex; align-items: center; gap: 10px; background: var(--primary); color: #fff; padding: 16px 20px; font-size: 1rem; font-weight: 700; mat-icon { font-size: 22px; } }
    .ofd-body { padding: 20px; display: flex; flex-direction: column; gap: 8px; }
    .ofd-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    mat-form-field { width: 100%; }
    .ofd-toggle { padding: 6px 0; }
    .ofd-actions { display: flex; gap: 10px; justify-content: flex-end; padding-top: 12px; border-top: 1px solid #f0f0f0; }
    .btn-cancel { background: none; border: 1px solid #ddd; border-radius: 8px; padding: 0 18px; height: 38px; cursor: pointer; font-size: .87rem; color: #666; &:hover { background: #f5f5f5; } }
    .btn-save { display: flex; align-items: center; gap: 6px; background: var(--primary); color: #fff; border: none; border-radius: 8px; padding: 0 20px; height: 38px; font-size: .87rem; font-weight: 600; cursor: pointer; &:hover { opacity: .9; } &:disabled { opacity: .5; } mat-icon { font-size: 18px; } }
  `],
})
export class OrganizationFormDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<OrganizationFormDialogComponent>);
  readonly data = inject<OrgFormDialogData>(MAT_DIALOG_DATA);

  form!: FormGroup;
  isEdit = false;

  ngOnInit(): void {
    this.isEdit = !!this.data?.organization;
    const o = this.data?.organization;
    this.form = this.fb.group({
      name: [o?.name ?? '', [Validators.required, Validators.maxLength(200)]],
      email: [o?.email ?? '', Validators.email],
      phone: [o?.phone ?? ''],
      address: [o?.address ?? ''],
      timezone: [o?.timezone ?? ''],
      currency: [o?.currency ?? 'INR'],
      logoUrl: [o?.logoUrl ?? ''],
      ...(this.isEdit ? { isActive: [o?.isActive ?? true] } : {}),
    });
  }

  submit(): void { if (this.form.invalid) { this.form.markAllAsTouched(); return; } this.dialogRef.close(this.form.value); }
  cancel(): void { this.dialogRef.close(); }
}

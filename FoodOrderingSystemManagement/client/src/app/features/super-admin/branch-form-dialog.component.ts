import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { Organization } from '../../core/models/organization.model';
import { Branch } from '../../core/models/branch.model';

export interface BranchFormDialogData { organization: Organization; branch?: Branch; }

@Component({
  selector: 'app-branch-form-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
            MatInputModule, MatButtonModule, MatIconModule, MatSlideToggleModule],
  template: `
    <div class="bfd-wrap">
      <div class="bfd-header">
        <mat-icon>store</mat-icon>
        <div>
          <span class="bfd-title">{{ isEdit() ? 'Edit Branch' : 'New Branch' }}</span>
          <span class="bfd-sub">{{ data.organization.name }}</span>
        </div>
      </div>
      <form [formGroup]="form" (ngSubmit)="submit()" class="bfd-body">
        <mat-form-field appearance="outline">
          <mat-label>Branch Name</mat-label>
          <input matInput formControlName="name" placeholder="e.g. Jubilee Hills" />
          <mat-icon matPrefix>store</mat-icon>
          @if (form.get('name')!.hasError('required') && form.get('name')!.touched) {
            <mat-error>Branch name is required</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Address</mat-label>
          <input matInput formControlName="address" placeholder="Street, area, city" />
          <mat-icon matPrefix>location_on</mat-icon>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Phone</mat-label>
          <input matInput formControlName="phone" placeholder="Branch contact number" />
          <mat-icon matPrefix>phone</mat-icon>
        </mat-form-field>

        @if (isEdit()) {
          <div class="bfd-toggle">
            <span>Active</span>
            <mat-slide-toggle formControlName="isActive"></mat-slide-toggle>
          </div>
        }

        <div class="bfd-info">
          <mat-icon>info</mat-icon>
          <span>This branch belongs to <strong>{{ data.organization.name }}</strong> and shares its menu, customers and settings — orders and tables stay separate per branch.</span>
        </div>

        <div class="bfd-actions">
          <button type="button" class="btn-cancel" (click)="cancel()">Cancel</button>
          <button type="submit" class="btn-create" [disabled]="form.invalid">
            <mat-icon>{{ isEdit() ? 'save' : 'add_business' }}</mat-icon> {{ isEdit() ? 'Save Changes' : 'Create Branch' }}
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .bfd-wrap { width: 460px; font-family: inherit; }

    .bfd-header {
      display: flex; align-items: center; gap: 14px;
      padding: 20px 24px; background: #1a237e; color: #fff;
      mat-icon { font-size: 26px; width: 26px; height: 26px; opacity: .9; }
      .bfd-title { display: block; font-size: 16px; font-weight: 700; }
      .bfd-sub   { display: block; font-size: 12px; opacity: .75; }
    }

    .bfd-body {
      padding: 20px 24px;
      display: flex; flex-direction: column; gap: 4px;
    }

    mat-form-field { width: 100%; }

    .bfd-toggle {
      display: flex; align-items: center; justify-content: space-between;
      padding: 8px 2px; margin-bottom: 8px; font-size: .87rem; color: #444;
    }

    .bfd-info {
      display: flex; align-items: flex-start; gap: 8px;
      padding: 12px 14px; background: #e8f0fe; border-radius: 8px;
      margin: 4px 0 8px; font-size: 13px; color: #1a237e; line-height: 1.5;
      mat-icon { font-size: 16px; width: 16px; height: 16px; flex-shrink: 0; margin-top: 1px; }
      strong { font-weight: 600; }
    }

    .bfd-actions {
      display: flex; gap: 10px; justify-content: flex-end;
      padding-top: 12px; border-top: 1px solid #f0f0f0;
    }

    .btn-cancel {
      background: none; border: 1px solid #ddd; border-radius: 8px;
      padding: 0 18px; height: 38px; cursor: pointer; font-size: .87rem; color: #666;
      &:hover { background: #f5f5f5; }
    }

    .btn-create {
      display: flex; align-items: center; gap: 6px;
      background: #1a237e; color: #fff; border: none; border-radius: 8px;
      padding: 0 20px; height: 38px; font-size: .87rem; font-weight: 600; cursor: pointer;
      &:hover { background: #283593; }
      &:disabled { opacity: .5; cursor: not-allowed; }
      mat-icon { font-size: 18px; width: 18px; height: 18px; }
    }
  `],
})
export class BranchFormDialogComponent {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<BranchFormDialogComponent>);
  readonly data = inject<BranchFormDialogData>(MAT_DIALOG_DATA);

  isEdit = signal(!!this.data.branch);

  form: FormGroup = this.fb.group({
    name:     [this.data.branch?.name ?? '', [Validators.required, Validators.maxLength(150)]],
    address:  [this.data.branch?.address ?? ''],
    phone:    [this.data.branch?.phone ?? ''],
    isActive: [this.data.branch?.isActive ?? true],
  });

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.dialogRef.close(this.form.value);
  }

  cancel(): void { this.dialogRef.close(); }
}

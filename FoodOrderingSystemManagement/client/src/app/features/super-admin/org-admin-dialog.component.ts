import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Organization } from '../../core/models/organization.model';
import { Branch } from '../../core/models/branch.model';

export interface OrgAdminDialogData { organization: Organization; branches: Branch[]; }

@Component({
  selector: 'app-org-admin-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
            MatInputModule, MatSelectModule, MatButtonModule, MatIconModule],
  template: `
    <div class="oad-wrap">
      <div class="oad-header">
        <mat-icon>person_add</mat-icon>
        <div>
          <span class="oad-title">Create Admin</span>
          <span class="oad-sub">{{ data.organization.name }}</span>
        </div>
      </div>
      <form [formGroup]="form" (ngSubmit)="submit()" class="oad-body">
        <mat-form-field appearance="outline">
          <mat-label>Full Name</mat-label>
          <input matInput formControlName="name" placeholder="John Doe" />
          <mat-icon matPrefix>person</mat-icon>
          @if (form.get('name')!.hasError('required') && form.get('name')!.touched) {
            <mat-error>Name is required</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Email Address</mat-label>
          <input matInput type="email" formControlName="email" placeholder="admin@restaurant.com" />
          <mat-icon matPrefix>email</mat-icon>
          @if (form.get('email')!.hasError('required') && form.get('email')!.touched) {
            <mat-error>Email is required</mat-error>
          }
          @if (form.get('email')!.hasError('email') && form.get('email')!.touched) {
            <mat-error>Enter a valid email</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Password</mat-label>
          <input matInput [type]="hide() ? 'password' : 'text'" formControlName="password" />
          <mat-icon matPrefix>lock</mat-icon>
          <button mat-icon-button matSuffix type="button" (click)="hide.set(!hide())">
            <mat-icon>{{ hide() ? 'visibility_off' : 'visibility' }}</mat-icon>
          </button>
          @if (form.get('password')!.hasError('required') && form.get('password')!.touched) {
            <mat-error>Password is required</mat-error>
          }
          @if (form.get('password')!.hasError('minlength') && form.get('password')!.touched) {
            <mat-error>Minimum 8 characters</mat-error>
          }
        </mat-form-field>

        @if (data.branches.length > 0) {
          <mat-form-field appearance="outline">
            <mat-label>Branch</mat-label>
            <mat-select formControlName="branchId">
              <mat-option [value]="null">All branches (org-wide Admin)</mat-option>
              @for (branch of data.branches; track branch.id) {
                <mat-option [value]="branch.id">{{ branch.name }}</mat-option>
              }
            </mat-select>
            <mat-icon matPrefix>store</mat-icon>
          </mat-form-field>
        }

        <div class="oad-info">
          <mat-icon>info</mat-icon>
          @if (form.get('branchId')!.value) {
            <span>This admin will only manage the selected branch of <strong>{{ data.organization.name }}</strong>.</span>
          } @else {
            <span>This admin will have full access to <strong>{{ data.organization.name }}</strong>'s restaurant management system — every branch combined.</span>
          }
        </div>

        <div class="oad-actions">
          <button type="button" class="btn-cancel" (click)="cancel()">Cancel</button>
          <button type="submit" class="btn-create" [disabled]="form.invalid">
            <mat-icon>person_add</mat-icon> Create Admin
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .oad-wrap { width: 460px; font-family: inherit; }

    .oad-header {
      display: flex; align-items: center; gap: 14px;
      padding: 20px 24px; background: #1a237e; color: #fff;
      mat-icon { font-size: 26px; width: 26px; height: 26px; opacity: .9; }
      .oad-title { display: block; font-size: 16px; font-weight: 700; }
      .oad-sub   { display: block; font-size: 12px; opacity: .75; }
    }

    .oad-body {
      padding: 20px 24px;
      display: flex; flex-direction: column; gap: 4px;
    }

    mat-form-field { width: 100%; }

    .oad-info {
      display: flex; align-items: flex-start; gap: 8px;
      padding: 12px 14px; background: #e8f0fe; border-radius: 8px;
      margin: 4px 0 8px; font-size: 13px; color: #1a237e; line-height: 1.5;
      mat-icon { font-size: 16px; width: 16px; height: 16px; flex-shrink: 0; margin-top: 1px; }
      strong { font-weight: 600; }
    }

    .oad-actions {
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
export class OrgAdminDialogComponent {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<OrgAdminDialogComponent>);
  readonly data = inject<OrgAdminDialogData>(MAT_DIALOG_DATA);

  hide = signal(true);

  form: FormGroup = this.fb.group({
    name:     ['', [Validators.required, Validators.maxLength(200)]],
    email:    ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    branchId: [null],
  });

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.dialogRef.close(this.form.value);
  }

  cancel(): void { this.dialogRef.close(); }
}

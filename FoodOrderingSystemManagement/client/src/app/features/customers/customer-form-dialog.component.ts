import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { CustomerService } from '../../core/services/customer.service';
import { Customer } from '../../core/models/customer.model';

@Component({
  selector: 'app-customer-form-dialog',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatButtonModule, MatDialogModule, MatFormFieldModule, MatInputModule,
    MatIconModule, MatProgressSpinnerModule, MatSlideToggleModule,
    MatDatepickerModule, MatNativeDateModule, MatSnackBarModule,
  ],
  template: `
    <h2 mat-dialog-title>{{ isEdit ? 'Edit Customer' : 'New Customer' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="form-grid">
        <mat-form-field appearance="outline">
          <mat-label>Full Name *</mat-label>
          <input matInput formControlName="name" />
          @if (form.get('name')?.hasError('required') && form.get('name')?.touched) {
            <mat-error>Name is required</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Phone *</mat-label>
          <input matInput formControlName="phone" />
          @if (form.get('phone')?.hasError('required') && form.get('phone')?.touched) {
            <mat-error>Phone is required</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Email</mat-label>
          <input matInput formControlName="email" type="email" />
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Birthday</mat-label>
          <input matInput [matDatepicker]="picker" formControlName="birthday" />
          <mat-datepicker-toggle matSuffix [for]="picker" />
          <mat-datepicker #picker />
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Notes</mat-label>
          <textarea matInput formControlName="notes" rows="3"></textarea>
        </mat-form-field>

        @if (isEdit) {
          <div class="toggle-row full-width">
            <label>Account Status</label>
            <mat-slide-toggle formControlName="isActive" color="primary">
              {{ form.get('isActive')?.value ? 'Active' : 'Inactive' }}
            </mat-slide-toggle>
          </div>
        }
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="primary" [disabled]="saving() || form.invalid" (click)="save()">
        @if (saving()) {
          <mat-spinner diameter="18" />
        } @else {
          <ng-container>
            <mat-icon>save</mat-icon>
            {{ isEdit ? 'Update' : 'Create' }}
          </ng-container>
        }
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .form-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 8px;
      min-width: 420px;
      padding-top: 8px;
    }
    .full-width { grid-column: 1 / -1; }
    mat-form-field { width: 100%; }
    .toggle-row {
      display: flex; align-items: center; justify-content: space-between;
      padding: 8px 0;
    }
    mat-spinner { display: inline-block; }
  `],
})
export class CustomerFormDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(CustomerService);
  private readonly dialogRef = inject(MatDialogRef<CustomerFormDialogComponent>);
  private readonly snack = inject(MatSnackBar);
  readonly customer: Customer | null = inject(MAT_DIALOG_DATA);

  form!: FormGroup;
  saving = signal(false);
  get isEdit() { return !!this.customer; }

  ngOnInit() {
    this.form = this.fb.group({
      name:     [this.customer?.name     ?? '', Validators.required],
      phone:    [this.customer?.phone    ?? '', Validators.required],
      email:    [this.customer?.email    ?? ''],
      birthday: [this.customer?.birthday ? new Date(this.customer.birthday) : null],
      notes:    [this.customer?.notes    ?? ''],
      isActive: [this.customer?.isActive ?? true],
    });

    if (this.isEdit) {
      this.form.get('phone')?.disable();
    }
  }

  save() {
    if (this.form.invalid) return;
    this.saving.set(true);
    const v = this.form.getRawValue();
    const birthday = v.birthday ? (v.birthday instanceof Date ? v.birthday.toISOString() : v.birthday) : undefined;

    if (this.isEdit) {
      this.service.update(this.customer!.id, { name: v.name, email: v.email || undefined, birthday, notes: v.notes || undefined, isActive: v.isActive }).subscribe({
        next: () => { this.saving.set(false); this.dialogRef.close(true); this.snack.open('Customer updated.', 'OK', { duration: 3000 }); },
        error: () => { this.saving.set(false); this.snack.open('Update failed.', 'OK', { duration: 3000 }); },
      });
    } else {
      this.service.create({ name: v.name, phone: v.phone, email: v.email || undefined, birthday, notes: v.notes || undefined }).subscribe({
        next: () => { this.saving.set(false); this.dialogRef.close(true); this.snack.open('Customer created.', 'OK', { duration: 3000 }); },
        error: err => { this.saving.set(false); this.snack.open(err?.error?.message ?? 'Create failed.', 'OK', { duration: 4000 }); },
      });
    }
  }
}

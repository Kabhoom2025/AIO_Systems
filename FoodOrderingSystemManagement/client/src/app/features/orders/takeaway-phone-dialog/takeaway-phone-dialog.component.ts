import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';

export interface TakeawayPhoneResult {
  phone: string | null; // null = skipped
}

@Component({
  selector: 'app-takeaway-phone-dialog',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatDialogModule, MatButtonModule, MatIconModule,
    MatInputModule, MatFormFieldModule,
  ],
  template: `
    <div class="tp-dialog">
      <div class="tp-header">
        <div class="tp-icon"><mat-icon>whatsapp</mat-icon></div>
        <div>
          <h2>Customer Phone</h2>
          <p>Enter mobile number to send order details via WhatsApp</p>
        </div>
      </div>

      <div class="tp-body">
        <div class="phone-input-row">
          <span class="country-code">+91</span>
          <input
            #phoneInput
            type="tel"
            class="phone-input"
            placeholder="98765 43210"
            maxlength="10"
            [(ngModel)]="phone"
            (keyup.enter)="canSubmit() && submit()"
            autocomplete="off"
          />
        </div>

        @if (phone().length > 0 && !isValid()) {
          <p class="error-hint">
            <mat-icon>info</mat-icon>
            Enter a valid 10-digit mobile number
          </p>
        }
      </div>

      <div class="tp-actions">
        <button class="btn-skip" (click)="skip()">
          Skip
        </button>
        <button class="btn-submit" [disabled]="!canSubmit()" (click)="submit()">
          <mat-icon>check</mat-icon>
          Place Order
        </button>
      </div>
    </div>
  `,
  styles: [`
    .tp-dialog { width: 360px; }

    .tp-header {
      display: flex; align-items: flex-start; gap: 14px;
      padding: 20px 20px 0;
    }
    .tp-icon {
      width: 44px; height: 44px; border-radius: 12px;
      background: #25D366; display: flex; align-items: center; justify-content: center;
      flex-shrink: 0;
      mat-icon { color: #fff; font-size: 24px; width: 24px; height: 24px; }
    }
    h2 { font-size: 1rem; font-weight: 700; color: #212121; margin: 0 0 3px; }
    p  { font-size: .82rem; color: #757575; margin: 0; }

    .tp-body { padding: 18px 20px 8px; }

    .phone-input-row {
      display: flex; align-items: center;
      border: 2px solid #e0e0e0; border-radius: 10px;
      overflow: hidden; transition: border-color .2s;
      &:focus-within { border-color: #25D366; }
    }
    .country-code {
      padding: 12px 10px 12px 14px;
      font-size: .9rem; font-weight: 700; color: #424242;
      background: #f5f5f5; border-right: 1.5px solid #e0e0e0;
      white-space: nowrap;
    }
    .phone-input {
      flex: 1; border: none; padding: 12px 14px;
      font-size: 1rem; font-weight: 600; color: #212121; outline: none;
      background: #fff; letter-spacing: .5px;
      &::placeholder { color: #bdbdbd; font-weight: 400; letter-spacing: 0; }
    }

    .error-hint {
      display: flex; align-items: center; gap: 5px;
      font-size: .78rem; color: #d32f2f; margin: 8px 0 0;
      mat-icon { font-size: 15px; width: 15px; height: 15px; }
    }

    .tp-actions {
      display: flex; gap: 8px;
      padding: 12px 20px 20px;
    }
    .btn-skip {
      flex: 1; padding: 11px; border-radius: 10px;
      border: 1.5px solid #e0e0e0; background: #fff;
      font-size: .88rem; font-weight: 600; color: #757575; cursor: pointer;
      transition: all .2s;
      &:hover { border-color: #bdbdbd; color: #424242; }
    }
    .btn-submit {
      flex: 2; display: flex; align-items: center; justify-content: center; gap: 6px;
      padding: 11px; border-radius: 10px; border: none;
      background: #25D366; color: #fff;
      font-size: .88rem; font-weight: 700; cursor: pointer;
      transition: background .2s;
      mat-icon { font-size: 18px; width: 18px; height: 18px; }
      &:hover:not(:disabled) { background: #1ebe5b; }
      &:disabled { opacity: .5; cursor: not-allowed; }
    }
  `],
})
export class TakeawayPhoneDialogComponent {
  private dialogRef = inject(MatDialogRef<TakeawayPhoneDialogComponent>);

  phone = signal('');

  isValid(): boolean {
    return /^[6-9]\d{9}$/.test(this.phone());
  }

  canSubmit(): boolean {
    return this.isValid();
  }

  skip(): void {
    this.dialogRef.close({ phone: null } as TakeawayPhoneResult);
  }

  submit(): void {
    this.dialogRef.close({ phone: this.phone() } as TakeawayPhoneResult);
  }
}

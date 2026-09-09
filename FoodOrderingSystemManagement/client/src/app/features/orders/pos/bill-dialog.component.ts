import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface BillDialogData {
  orderNumber: string;
  tableNumber: number | null;
  subTotal: number;
  tax: number;
  currentDiscount: number;
}

export interface BillDialogResult {
  discount: number;
}

@Component({
  selector: 'app-bill-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, MatDialogModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule],
  template: `
    <div class="bill-dialog">
      <div class="bd-header">
        <mat-icon>receipt</mat-icon>
        <span>Generate Bill</span>
      </div>

      <div class="bd-body">
        <div class="bd-order">
          <span class="bd-label">Order</span>
          <span class="bd-value">#{{ data.orderNumber }} — Table {{ data.tableNumber }}</span>
        </div>
        <div class="bd-row">
          <span>Subtotal</span><span>₹{{ data.subTotal | number:'1.2-2' }}</span>
        </div>
        <div class="bd-row">
          <span>Tax</span><span>₹{{ data.tax | number:'1.2-2' }}</span>
        </div>

        <div class="bd-discount-section">
          <div class="bd-discount-label">
            <mat-icon>local_offer</mat-icon>
            <span>Discount (optional)</span>
          </div>
          <div class="bd-discount-row">
            <button class="mode-btn" [class.active]="mode() === 'amount'"   (click)="mode.set('amount')">₹ Amount</button>
            <button class="mode-btn" [class.active]="mode() === 'percent'" (click)="mode.set('percent')">% Percent</button>
            <input class="discount-input" type="number" min="0"
                   [(ngModel)]="discountInput"
                   (ngModelChange)="recalc()"
                   [placeholder]="mode() === 'percent' ? '0–100' : '0.00'" />
          </div>
        </div>

        <div class="bd-total-row">
          <span>Grand Total</span>
          <span class="bd-grand">₹{{ grandTotal() | number:'1.2-2' }}</span>
        </div>
        @if (discountAmt() > 0) {
          <div class="bd-saving">You saved ₹{{ discountAmt() | number:'1.2-2' }}</div>
        }
      </div>

      <div class="bd-actions">
        <button class="btn-cancel" (click)="ref.close(null)">Cancel</button>
        <button class="btn-confirm" (click)="confirm()">
          <mat-icon>print</mat-icon>
          Generate &amp; Print Bill
        </button>
      </div>
    </div>
  `,
  styles: [`
    .bill-dialog { width: 380px; font-family: inherit; }

    .bd-header {
      display: flex; align-items: center; gap: 10px;
      background: var(--primary); color: #fff;
      padding: 16px 20px; font-size: 1rem; font-weight: 700;
      mat-icon { font-size: 22px; }
    }

    .bd-body { padding: 18px 20px; }

    .bd-order {
      display: flex; flex-direction: column; margin-bottom: 14px;
      .bd-label { font-size: .72rem; text-transform: uppercase; color: #aaa; font-weight: 600; }
      .bd-value { font-size: .95rem; font-weight: 700; color: #222; }
    }

    .bd-row {
      display: flex; justify-content: space-between;
      font-size: .88rem; color: #555; padding: 4px 0;
    }

    .bd-discount-section {
      background: #fff8f0; border: 1px solid #ffe0b2;
      border-radius: 10px; padding: 12px 14px; margin: 14px 0;
    }

    .bd-discount-label {
      display: flex; align-items: center; gap: 6px;
      font-size: .8rem; font-weight: 700; color: #e65100; margin-bottom: 10px;
      mat-icon { font-size: 16px; width: 16px; height: 16px; }
    }

    .bd-discount-row {
      display: flex; align-items: center; gap: 8px;
    }

    .mode-btn {
      border: 1.5px solid #ddd; background: #fff; border-radius: 6px;
      padding: 4px 10px; font-size: .78rem; cursor: pointer; white-space: nowrap;
      &.active { border-color: var(--primary); background: #fff0f0; color: var(--primary); font-weight: 700; }
    }

    .discount-input {
      flex: 1; border: 1.5px solid #ddd; border-radius: 6px;
      padding: 5px 10px; font-size: .9rem; outline: none;
      &:focus { border-color: var(--primary); }
    }

    .bd-total-row {
      display: flex; justify-content: space-between; align-items: center;
      padding: 10px 0 4px; border-top: 1.5px dashed #e0e0e0; margin-top: 4px;
      font-size: .95rem; font-weight: 700;
    }

    .bd-grand { font-size: 1.2rem; color: var(--primary); font-weight: 800; }

    .bd-saving {
      text-align: right; font-size: .78rem; color: #2e7d32; font-weight: 600;
    }

    .bd-actions {
      display: flex; gap: 10px; justify-content: flex-end;
      padding: 12px 20px; border-top: 1px solid #f0f0f0; background: #fafafa;
    }

    .btn-cancel {
      background: none; border: 1px solid #ddd; border-radius: 8px;
      padding: 0 18px; height: 38px; cursor: pointer; font-size: .87rem; color: #666;
    }

    .btn-confirm {
      display: flex; align-items: center; gap: 7px;
      background: #1b5e20; color: #fff; border: none; border-radius: 8px;
      padding: 0 20px; height: 38px; font-size: .87rem; font-weight: 600; cursor: pointer;
      mat-icon { font-size: 18px; }
      &:hover { background: #2e7d32; }
    }
  `],
})
export class BillDialogComponent {
  readonly ref  = inject(MatDialogRef<BillDialogComponent>);
  readonly data = inject<BillDialogData>(MAT_DIALOG_DATA);

  mode         = signal<'amount' | 'percent'>('amount');
  discountInput = 0;

  discountAmt = signal(0);

  grandTotal = signal(this.data.subTotal + this.data.tax);

  recalc(): void {
    const val = parseFloat(String(this.discountInput)) || 0;
    let amt = 0;
    if (this.mode() === 'percent') {
      amt = parseFloat(((this.data.subTotal * Math.min(val, 100)) / 100).toFixed(2));
    } else {
      amt = Math.max(0, val);
    }
    this.discountAmt.set(amt);
    this.grandTotal.set(parseFloat((this.data.subTotal + this.data.tax - amt).toFixed(2)));
  }

  confirm(): void {
    this.ref.close({ discount: this.discountAmt() } as BillDialogResult);
  }
}

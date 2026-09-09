import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import {
  LedgerType,
  CreateLedgerEntryRequest,
  CREDIT_CATEGORIES,
  DEBIT_CATEGORIES,
} from '../../core/models/ledger.model';

interface DialogData { date: string; }

@Component({
  selector: 'app-ledger-entry-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonToggleModule,
  ],
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <mat-icon>add_circle</mat-icon>
        <h2>Add Ledger Entry</h2>
        <span class="date-chip">{{ data.date }}</span>
      </div>

      <div class="dialog-body">
        <!-- Type toggle -->
        <div class="type-toggle">
          <button class="toggle-btn credit" [class.active]="type() === 'Credit'" (click)="setType('Credit')">
            <mat-icon>arrow_downward</mat-icon>
            Credit (In)
          </button>
          <button class="toggle-btn debit" [class.active]="type() === 'Debit'" (click)="setType('Debit')">
            <mat-icon>arrow_upward</mat-icon>
            Debit (Out)
          </button>
        </div>

        <!-- Category -->
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Category</mat-label>
          <mat-select [(ngModel)]="category" required>
            @for (cat of categories(); track cat) {
              <mat-option [value]="cat">{{ cat }}</mat-option>
            }
          </mat-select>
          <mat-icon matSuffix>label</mat-icon>
        </mat-form-field>

        <!-- Amount -->
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Amount (₹)</mat-label>
          <input matInput type="number" min="0.01" step="0.01" [(ngModel)]="amount" required placeholder="0.00" />
          <mat-icon matSuffix>currency_rupee</mat-icon>
        </mat-form-field>

        <!-- Note -->
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Note (optional)</mat-label>
          <textarea matInput rows="2" [(ngModel)]="note" maxlength="300" placeholder="Short description…"></textarea>
          <mat-icon matSuffix>notes</mat-icon>
        </mat-form-field>
      </div>

      <div class="dialog-footer">
        <button mat-stroked-button (click)="cancel()">Cancel</button>
        <button mat-raised-button [color]="type() === 'Credit' ? 'primary' : 'warn'"
                [disabled]="!isValid()" (click)="submit()">
          <mat-icon>save</mat-icon>
          Save {{ type() }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .dialog-container { width: 460px; font-family: inherit; }

    .dialog-header {
      display: flex; align-items: center; gap: 10px;
      padding: 20px 24px 16px;
      border-bottom: 1px solid #f0f0f0;
      mat-icon { font-size: 26px; color: var(--primary); }
      h2 { margin: 0; font-size: 1.1rem; font-weight: 700; flex: 1; }
    }

    .date-chip {
      background: #f3f4f6; border-radius: 8px; padding: 3px 10px;
      font-size: .78rem; font-weight: 600; color: #616161;
    }

    .dialog-body {
      padding: 20px 24px;
      display: flex; flex-direction: column; gap: 14px;
    }

    .type-toggle {
      display: grid; grid-template-columns: 1fr 1fr; gap: 8px;
    }

    .toggle-btn {
      display: flex; align-items: center; justify-content: center; gap: 6px;
      padding: 10px 0; border-radius: 10px; border: 2px solid #e0e0e0;
      background: #fafafa; font-size: .9rem; font-weight: 600; cursor: pointer;
      transition: all .15s;
      mat-icon { font-size: 18px; }

      &.credit { color: #616161;
        &.active { border-color: #4caf50; background: #e8f5e9; color: #2e7d32; } }
      &.debit  { color: #616161;
        &.active { border-color: #f44336; background: #ffebee; color: #c62828; } }
      &:hover:not(.active) { border-color: #bdbdbd; }
    }

    .full-width { width: 100%; }

    .dialog-footer {
      display: flex; justify-content: flex-end; gap: 10px;
      padding: 12px 24px 20px;
      border-top: 1px solid #f0f0f0;
    }
  `],
})
export class LedgerEntryDialogComponent implements OnInit {
  private dialogRef = inject(MatDialogRef<LedgerEntryDialogComponent>);
  data: DialogData = inject(MAT_DIALOG_DATA);

  type     = signal<LedgerType>('Credit');
  category = '';
  amount: number | null = null;
  note     = '';

  categories = computed(() =>
    this.type() === 'Credit' ? CREDIT_CATEGORIES : DEBIT_CATEGORIES
  );

  ngOnInit(): void { this.category = this.categories()[0]; }

  setType(t: LedgerType): void {
    this.type.set(t);
    this.category = this.categories()[0];
  }

  isValid(): boolean {
    return !!this.category && !!this.amount && this.amount > 0;
  }

  submit(): void {
    if (!this.isValid()) return;
    const payload: CreateLedgerEntryRequest = {
      date:     this.data.date,
      type:     this.type(),
      amount:   this.amount!,
      category: this.category,
      note:     this.note || undefined,
    };
    this.dialogRef.close(payload);
  }

  cancel(): void { this.dialogRef.close(null); }
}

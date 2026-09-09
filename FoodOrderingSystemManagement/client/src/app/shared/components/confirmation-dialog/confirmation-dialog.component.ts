import { Component, inject } from '@angular/core';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  danger?: boolean;
}

@Component({
  selector: 'app-confirmation-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <h2 mat-dialog-title class="confirm-title" [class.danger]="data.danger">
      <mat-icon>{{ data.danger ? 'warning' : 'help_outline' }}</mat-icon>
      {{ data.title }}
    </h2>
    <mat-dialog-content>
      <p>{{ data.message }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-stroked-button (click)="close(false)">
        {{ data.cancelText || 'Cancel' }}
      </button>
      <button mat-raised-button [color]="data.danger ? 'warn' : 'primary'" (click)="close(true)">
        {{ data.confirmText || 'Confirm' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .confirm-title {
      display: flex; align-items: center; gap: 10px; font-size: 1.1rem;
      mat-icon { color: var(--primary); }
      &.danger mat-icon { color: #d32f2f; }
    }
    mat-dialog-content p { color: #424242; margin: 0; line-height: 1.6; }
    mat-dialog-actions { gap: 8px; padding: 16px 24px !important; }
  `],
})
export class ConfirmationDialogComponent {
  data: ConfirmDialogData = inject(MAT_DIALOG_DATA);
  private dialogRef = inject(MatDialogRef<ConfirmationDialogComponent>);

  close(result: boolean): void {
    this.dialogRef.close(result);
  }
}

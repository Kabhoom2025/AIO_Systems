import { Component, inject, OnInit, ViewChild, ElementRef, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import * as QRCode from 'qrcode';
import { OrderService } from '../../../core/services/order.service';
import { NotificationService } from '../../../shared/services/notification.service';

export interface PaymentQrDialogData {
  orderId:        number;
  orderNumber:    string;
  grandTotal:     number;
  upiId?:         string | null;
  restaurantName: string;
}

@Component({
  selector: 'app-payment-qr-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  template: `
    <div class="pqr-wrap">

      <!-- Header -->
      <div class="pqr-header">
        <div class="pqr-header-left">
          <mat-icon>qr_code_scanner</mat-icon>
          <div>
            <span class="pqr-title">Payment</span>
            <span class="pqr-sub">{{ data.orderNumber }}</span>
          </div>
        </div>
        <button mat-icon-button (click)="close()" class="pqr-close"><mat-icon>close</mat-icon></button>
      </div>

      <!-- Amount band -->
      <div class="pqr-amount-band">
        <span class="pqr-label">Total Amount</span>
        <span class="pqr-amount">₹{{ data.grandTotal | number:'1.2-2' }}</span>
      </div>

      <!-- QR section -->
      <div class="pqr-qr-section">
        @if (data.upiId) {
          <canvas #qrCanvas class="pqr-canvas"></canvas>
          <p class="pqr-upi-hint">Scan to pay via UPI</p>
          <p class="pqr-upi-id">{{ data.upiId }}</p>
        } @else {
          <div class="pqr-no-upi">
            <mat-icon>qr_code_2</mat-icon>
            <p>UPI ID not configured</p>
            <span>Go to Settings → UPI ID to enable QR payments</span>
          </div>
        }
      </div>

      <!-- Actions -->
      <div class="pqr-footer">
        <button mat-stroked-button (click)="close()">Close</button>
        <button mat-raised-button color="primary" (click)="confirmPaid()" [disabled]="confirming()">
          @if (confirming()) {
            <mat-spinner diameter="16" strokeWidth="2"></mat-spinner>
            &nbsp;Confirming…
          } @else {
            <ng-container>
              <mat-icon>check_circle</mat-icon>
              Paid
            </ng-container>
          }
        </button>
      </div>

    </div>
  `,
  styles: [`
    .pqr-wrap { width: 360px; font-family: inherit; display: flex; flex-direction: column; }

    .pqr-header {
      display: flex; align-items: center; justify-content: space-between;
      background: #2e7d32; color: #fff; padding: 14px 16px 14px 20px;
      mat-icon { font-size: 22px; }
    }
    .pqr-header-left { display: flex; align-items: center; gap: 12px; }
    .pqr-title { display: block; font-size: 1rem; font-weight: 700; line-height: 1.2; }
    .pqr-sub   { display: block; font-size: .72rem; opacity: .8; margin-top: 1px; }
    .pqr-close { color: #fff !important; }

    .pqr-amount-band {
      display: flex; align-items: center; justify-content: space-between;
      padding: 14px 20px; background: #f1f8e9; border-bottom: 1px solid #c8e6c9;
    }
    .pqr-label  { font-size: .78rem; color: #558b2f; font-weight: 600; }
    .pqr-amount { font-size: 1.6rem; font-weight: 800; color: #1b5e20; font-variant-numeric: tabular-nums; }

    .pqr-qr-section {
      display: flex; flex-direction: column; align-items: center; justify-content: center;
      padding: 24px 20px 20px; min-height: 240px;
    }
    .pqr-canvas { border: 4px solid #e8f5e9; border-radius: 12px; display: block; }
    .pqr-upi-hint { font-size: .78rem; color: #757575; margin: 10px 0 2px; }
    .pqr-upi-id   { font-size: .8rem; font-weight: 600; color: #2e7d32; margin: 0; }

    .pqr-no-upi {
      display: flex; flex-direction: column; align-items: center; gap: 8px;
      color: #9e9e9e; text-align: center;
      mat-icon { font-size: 48px; width: 48px; height: 48px; color: #e0e0e0; }
      p    { margin: 0; font-weight: 600; font-size: .9rem; color: #616161; }
      span { font-size: .75rem; color: #9e9e9e; }
    }

    .pqr-footer {
      display: flex; justify-content: flex-end; gap: 10px;
      padding: 12px 16px; border-top: 1px solid #f0f0f0;
      button[color=primary] { gap: 6px; display: flex; align-items: center; }
    }
  `],
})
export class PaymentQrDialogComponent implements OnInit {
  @ViewChild('qrCanvas') qrCanvas?: ElementRef<HTMLCanvasElement>;

  private dialogRef    = inject(MatDialogRef<PaymentQrDialogComponent>);
  readonly data        = inject<PaymentQrDialogData>(MAT_DIALOG_DATA);
  private orderService = inject(OrderService);
  private notify       = inject(NotificationService);

  confirming = signal(false);

  ngOnInit(): void {
    if (this.data.upiId) {
      setTimeout(() => this.renderQr(), 50);
    }
  }

  private renderQr(): void {
    if (!this.qrCanvas?.nativeElement) return;
    const upiString =
      `upi://pay?pa=${encodeURIComponent(this.data.upiId!)}` +
      `&pn=${encodeURIComponent(this.data.restaurantName)}` +
      `&am=${this.data.grandTotal.toFixed(2)}` +
      `&cu=INR` +
      `&tn=${encodeURIComponent(this.data.orderNumber)}`;

    QRCode.toCanvas(this.qrCanvas.nativeElement, upiString, {
      width: 200,
      margin: 2,
      color: { dark: '#1b5e20', light: '#ffffff' },
    }).catch(() => {});
  }

  confirmPaid(): void {
    this.confirming.set(true);
    this.orderService.confirmPayment(this.data.orderId).subscribe({
      next: () => {
        this.confirming.set(false);
        this.notify.success('Payment confirmed!');
        this.dialogRef.close({ paid: true });
      },
      error: (err) => {
        this.confirming.set(false);
        this.notify.error(err?.error?.message || 'Failed to confirm payment.');
      },
    });
  }

  close(): void { this.dialogRef.close({ paid: false }); }
}

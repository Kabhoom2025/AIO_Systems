import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface QrDialogData {
  tableNumber: number;
  menuUrl: string;
}

@Component({
  selector: 'app-qr-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="qr-dialog">
      <div class="qr-header">
        <mat-icon>qr_code_2</mat-icon>
        <span>Table {{ data.tableNumber }} — QR Code</span>
      </div>

      <div class="qr-body">
        <p class="qr-hint">Customers can scan this to view the menu on their phone</p>

        @if (loading()) {
          <div class="qr-loading">Generating QR code...</div>
        } @else if (qrDataUrl()) {
          <div class="qr-img-wrap">
            <img [src]="qrDataUrl()!" alt="QR Code" class="qr-img" />
          </div>
          <p class="qr-url">{{ data.menuUrl }}</p>
        }
      </div>

      <div class="qr-actions">
        <button mat-button (click)="ref.close()">Close</button>
        <button mat-raised-button color="primary" (click)="print()" [disabled]="!qrDataUrl()">
          <mat-icon>print</mat-icon> Print QR
        </button>
        @if (qrDataUrl()) {
          <a [href]="qrDataUrl()!" [download]="'table-' + data.tableNumber + '-qr.png'" mat-stroked-button>
            <mat-icon>download</mat-icon> Download
          </a>
        }
      </div>
    </div>
  `,
  styles: [`
    .qr-dialog { width: 340px; font-family: inherit; }

    .qr-header {
      display: flex; align-items: center; gap: 10px;
      background: #1a237e; color: #fff;
      padding: 16px 20px; font-size: 1rem; font-weight: 700;
      mat-icon { font-size: 24px; }
    }

    .qr-body { padding: 20px; text-align: center; }

    .qr-hint {
      font-size: .82rem; color: #666; margin: 0 0 16px;
    }

    .qr-loading { color: #aaa; font-size: .9rem; padding: 40px 0; }

    .qr-img-wrap {
      display: inline-flex;
      padding: 12px;
      background: #fff;
      border: 2px solid #e0e0e0;
      border-radius: 12px;
      box-shadow: 0 2px 8px rgba(0,0,0,.08);
    }

    .qr-img { width: 200px; height: 200px; display: block; }

    .qr-url {
      margin-top: 12px;
      font-size: .72rem;
      color: #1a237e;
      word-break: break-all;
      background: #e8eaf6;
      border-radius: 6px;
      padding: 6px 10px;
    }

    .qr-actions {
      display: flex; gap: 8px; justify-content: flex-end; flex-wrap: wrap;
      padding: 12px 16px; border-top: 1px solid #f0f0f0; background: #fafafa;
    }
  `],
})
export class QrDialogComponent implements OnInit {
  readonly ref  = inject(MatDialogRef<QrDialogComponent>);
  readonly data = inject<QrDialogData>(MAT_DIALOG_DATA);

  loading   = signal(true);
  qrDataUrl = signal<string | null>(null);

  async ngOnInit(): Promise<void> {
    try {
      const QRCode = (await import('qrcode')).default;
      const url = await QRCode.toDataURL(this.data.menuUrl, {
        width: 400,
        margin: 2,
        color: { dark: '#1a237e', light: '#ffffff' },
      });
      this.qrDataUrl.set(url);
    } catch {
      this.qrDataUrl.set(null);
    }
    this.loading.set(false);
  }

  print(): void {
    const url  = this.qrDataUrl();
    if (!url) return;
    const win  = window.open('', '_blank', 'width=420,height=500');
    if (!win) return;
    win.document.write(`
      <!DOCTYPE html>
      <html>
      <head>
        <title>Table ${this.data.tableNumber} QR Code</title>
        <style>
          body { margin: 0; display: flex; flex-direction: column; align-items: center;
                 justify-content: center; min-height: 100vh; font-family: sans-serif; }
          .card { border: 2px solid #1a237e; border-radius: 12px; padding: 24px;
                  text-align: center; max-width: 320px; }
          h2 { color: #1a237e; margin: 0 0 4px; font-size: 1.2rem; }
          p  { color: #555; font-size: .8rem; margin: 0 0 16px; }
          img { width: 240px; height: 240px; }
          .url { font-size: .65rem; color: #1a237e; margin-top: 10px; word-break: break-all; }
        </style>
      </head>
      <body>
        <div class="card">
          <h2>Table ${this.data.tableNumber}</h2>
          <p>Scan to view our menu</p>
          <img src="${url}" />
          <div class="url">${this.data.menuUrl}</div>
        </div>
      </body>
      </html>
    `);
    win.document.close();
    win.focus();
    setTimeout(() => win.print(), 300);
  }
}

import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.model';
import { Table } from '../../../core/models/table.model';

@Component({
  selector: 'app-generate-qr',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatIconModule, MatButtonModule,
    MatFormFieldModule, MatInputModule, MatSelectModule,
    MatButtonToggleModule, MatProgressSpinnerModule, MatSnackBarModule,
  ],
  templateUrl: './generate-qr.component.html',
  styleUrl: './generate-qr.component.scss',
})
export class GenerateQrComponent implements OnInit {
  private http  = inject(HttpClient);
  private snack = inject(MatSnackBar);

  // Form-bound properties
  mode: 'table' | 'custom' = 'table';
  selectedTableNumber: number | null = null;
  customUrl = '';
  qrSize    = 300;
  qrColor   = '#1a237e';

  // Reactive state
  generating    = signal(false);
  qrDataUrl     = signal<string | null>(null);
  tables        = signal<Table[]>([]);
  loadingTables = signal(true);

  readonly Math = Math;

  get qrUrl(): string {
    if (this.mode === 'table' && this.selectedTableNumber != null) {
      return `${window.location.origin}/menu?table=${this.selectedTableNumber}`;
    }
    return this.customUrl;
  }

  ngOnInit(): void {
    this.http.get<ApiResponse<Table[]>>(`${environment.apiUrl}/table`).subscribe({
      next: res => {
        const active = (res.data ?? []).filter(t => t.isActive);
        this.tables.set(active);
        this.loadingTables.set(false);
        if (active.length > 0) {
          this.selectedTableNumber = active[0].tableNumber;
          this.generateQr();
        }
      },
      error: () => this.loadingTables.set(false),
    });
  }

  async generateQr(): Promise<void> {
    const url = this.qrUrl;
    if (!url) return;
    this.generating.set(true);
    try {
      const QRCode = (await import('qrcode')).default;
      const dataUrl = await QRCode.toDataURL(url, {
        width: this.qrSize,
        margin: 2,
        color: { dark: this.qrColor, light: '#ffffff' },
      });
      this.qrDataUrl.set(dataUrl);
    } catch {
      this.qrDataUrl.set(null);
    }
    this.generating.set(false);
  }

  onModeChange(): void {
    this.qrDataUrl.set(null);
  }

  download(): void {
    const url = this.qrDataUrl();
    if (!url) return;
    const label = this.mode === 'table' ? `table-${this.selectedTableNumber}` : 'custom';
    const a = document.createElement('a');
    a.href = url;
    a.download = `qr-${label}.png`;
    a.click();
  }

  print(): void {
    const dataUrl = this.qrDataUrl();
    if (!dataUrl) return;
    const label   = this.mode === 'table' ? `Table ${this.selectedTableNumber}` : 'Custom QR';
    const color   = this.qrColor;
    const qrLink  = this.qrUrl;
    const size    = this.qrSize;
    const win = window.open('', '_blank', 'width=460,height=540');
    if (!win) return;
    win.document.write(`<!DOCTYPE html><html><head><title>${label} QR Code</title>
      <style>
        body { margin:0; display:flex; flex-direction:column; align-items:center;
               justify-content:center; min-height:100vh; font-family:sans-serif; background:#fff; }
        .card { border:2px solid ${color}; border-radius:14px; padding:28px; text-align:center; }
        h2 { color:${color}; margin:0 0 4px; font-size:1.2rem; }
        p  { color:#555; font-size:.82rem; margin:0 0 18px; }
        img { width:${size}px; height:${size}px; }
        .url { font-size:.65rem; color:#444; margin-top:12px; word-break:break-all; }
      </style></head><body>
      <div class="card">
        <h2>${label}</h2><p>Scan to view our menu</p>
        <img src="${dataUrl}" />
        <div class="url">${qrLink}</div>
      </div></body></html>`);
    win.document.close(); win.focus();
    setTimeout(() => win.print(), 300);
  }

  copyUrl(): void {
    const url = this.qrUrl;
    if (!url) return;
    navigator.clipboard.writeText(url).then(() =>
      this.snack.open('URL copied!', '', { duration: 2000 })
    );
  }
}

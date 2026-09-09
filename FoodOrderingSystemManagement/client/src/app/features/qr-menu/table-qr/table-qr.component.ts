import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.model';
import { Table } from '../../../core/models/table.model';

@Component({
  selector: 'app-table-qr',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatIconModule, MatButtonModule,
    MatProgressSpinnerModule, MatButtonToggleModule, MatTooltipModule, MatSnackBarModule,
  ],
  templateUrl: './table-qr.component.html',
  styleUrl: './table-qr.component.scss',
})
export class TableQrComponent implements OnInit {
  private http  = inject(HttpClient);
  private snack = inject(MatSnackBar);

  loading       = signal(true);
  generating    = signal(false);
  tables        = signal<Table[]>([]);
  qrMap         = signal<Map<number, string>>(new Map());
  hallFilter    = signal<'All' | 'AC' | 'Non-AC'>('All');

  filteredTables = computed(() => {
    const f = this.hallFilter();
    return f === 'All' ? this.tables() : this.tables().filter(t => t.hall === f);
  });

  ngOnInit(): void {
    this.http.get<ApiResponse<Table[]>>(`${environment.apiUrl}/table`).subscribe({
      next: async res => {
        const active = (res.data ?? []).filter(t => t.isActive)
          .sort((a, b) => a.tableNumber - b.tableNumber);
        this.tables.set(active);
        this.loading.set(false);
        await this.buildQrMap(active);
      },
      error: () => this.loading.set(false),
    });
  }

  private async buildQrMap(tables: Table[]): Promise<void> {
    this.generating.set(true);
    const QRCode = (await import('qrcode')).default;
    const map = new Map<number, string>();
    const origin = window.location.origin;
    for (const t of tables) {
      const url = `${origin}/menu?table=${t.tableNumber}`;
      map.set(t.id, await QRCode.toDataURL(url, {
        width: 220, margin: 2,
        color: { dark: '#1a237e', light: '#ffffff' },
      }));
    }
    this.qrMap.set(map);
    this.generating.set(false);
  }

  getQr(tableId: number): string | null {
    return this.qrMap().get(tableId) ?? null;
  }

  menuUrl(tableNumber: number): string {
    return `${window.location.origin}/menu?table=${tableNumber}`;
  }

  download(t: Table): void {
    const dataUrl = this.getQr(t.id);
    if (!dataUrl) return;
    const a = document.createElement('a');
    a.href = dataUrl;
    a.download = `table-${t.tableNumber}-qr.png`;
    a.click();
  }

  printCard(t: Table): void {
    const dataUrl = this.getQr(t.id);
    if (!dataUrl) return;
    const url = this.menuUrl(t.tableNumber);
    const win = window.open('', '_blank', 'width=440,height=520');
    if (!win) return;
    win.document.write(this.buildPrintHtml(`Table ${t.tableNumber}`, t.hall, url, dataUrl));
    win.document.close(); win.focus();
    setTimeout(() => win.print(), 300);
  }

  printAll(): void {
    const tables = this.filteredTables();
    const origin = window.location.origin;
    let cards = '';
    for (const t of tables) {
      const dataUrl = this.getQr(t.id);
      if (!dataUrl) continue;
      cards += `
        <div class="card">
          <h3>Table ${t.tableNumber}</h3>
          <p>${t.hall} &bull; ${t.capacity} seats</p>
          <img src="${dataUrl}" />
          <div class="url">${origin}/menu?table=${t.tableNumber}</div>
        </div>`;
    }
    const win = window.open('', '_blank');
    if (!win) return;
    win.document.write(`<!DOCTYPE html><html><head><title>All Table QR Codes</title>
      <style>
        body { font-family: sans-serif; margin: 20px; }
        .grid { display: flex; flex-wrap: wrap; gap: 20px; }
        .card { border: 2px solid #1a237e; border-radius: 12px; padding: 16px;
                text-align: center; width: 200px; break-inside: avoid; }
        h3 { color: #1a237e; margin: 0 0 2px; font-size: 1rem; }
        p  { color: #555; font-size: .75rem; margin: 0 0 12px; }
        img { width: 160px; height: 160px; }
        .url { font-size: .55rem; color: #444; margin-top: 8px; word-break: break-all; }
        @media print { .no-print { display: none; } }
      </style></head><body>
      <h1 class="no-print" style="color:#1a237e">All Table QR Codes</h1>
      <div class="grid">${cards}</div>
      </body></html>`);
    win.document.close(); win.focus();
    setTimeout(() => win.print(), 400);
  }

  private buildPrintHtml(label: string, hall: string, url: string, dataUrl: string): string {
    return `<!DOCTYPE html><html><head><title>${label} QR</title>
      <style>
        body { margin:0; display:flex; flex-direction:column; align-items:center;
               justify-content:center; min-height:100vh; font-family:sans-serif; }
        .card { border:2px solid #1a237e; border-radius:14px; padding:28px; text-align:center; }
        h2 { color:#1a237e; margin:0 0 4px; }
        p  { color:#555; font-size:.8rem; margin:0 0 16px; }
        img { width:220px; height:220px; }
        .url { font-size:.62rem; color:#444; margin-top:10px; word-break:break-all; }
      </style></head><body>
      <div class="card">
        <h2>${label}</h2><p>${hall} &bull; Scan to view our menu</p>
        <img src="${dataUrl}" />
        <div class="url">${url}</div>
      </div></body></html>`;
  }

  halls = computed(() => {
    const set = new Set(this.tables().map(t => t.hall));
    return ['All', ...Array.from(set)] as ('All' | 'AC' | 'Non-AC')[];
  });
}

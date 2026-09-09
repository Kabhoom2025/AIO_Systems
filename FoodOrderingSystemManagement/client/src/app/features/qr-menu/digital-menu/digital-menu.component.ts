import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-digital-menu',
  standalone: true,
  imports: [
    CommonModule, MatIconModule, MatButtonModule,
    MatProgressSpinnerModule, MatSnackBarModule,
  ],
  templateUrl: './digital-menu.component.html',
  styleUrl: './digital-menu.component.scss',
})
export class DigitalMenuComponent implements OnInit {
  private snack     = inject(MatSnackBar);
  private sanitizer = inject(DomSanitizer);

  qrDataUrl      = signal<string | null>(null);
  generatingQr   = signal(true);
  previewLoaded  = signal(false);

  readonly menuUrl: string = `${window.location.origin}/menu`;
  readonly safeMenuUrl: SafeResourceUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.menuUrl);

  async ngOnInit(): Promise<void> {
    try {
      const QRCode = (await import('qrcode')).default;
      const dataUrl = await QRCode.toDataURL(this.menuUrl, {
        width: 280, margin: 2,
        color: { dark: '#1a237e', light: '#ffffff' },
      });
      this.qrDataUrl.set(dataUrl);
    } catch {
      this.qrDataUrl.set(null);
    }
    this.generatingQr.set(false);
  }

  copyUrl(): void {
    navigator.clipboard.writeText(this.menuUrl).then(() =>
      this.snack.open('Link copied!', '', { duration: 2000 })
    );
  }

  openMenu(): void {
    window.open(this.menuUrl, '_blank');
  }

  download(): void {
    const url = this.qrDataUrl();
    if (!url) return;
    const a = document.createElement('a');
    a.href = url;
    a.download = 'menu-qr.png';
    a.click();
  }

  print(): void {
    const dataUrl = this.qrDataUrl();
    if (!dataUrl) return;
    const menuUrl = this.menuUrl;
    const win = window.open('', '_blank', 'width=460,height=560');
    if (!win) return;
    win.document.write(`<!DOCTYPE html><html><head><title>Digital Menu QR</title>
      <style>
        body { margin:0; display:flex; flex-direction:column; align-items:center;
               justify-content:center; min-height:100vh; font-family:sans-serif; background:#fff; }
        .card { border:2px solid #1a237e; border-radius:16px; padding:32px; text-align:center; max-width:320px; }
        h2 { color:#1a237e; margin:0 0 6px; font-size:1.3rem; }
        p  { color:#555; font-size:.85rem; margin:0 0 20px; }
        img { width:280px; height:280px; }
        .url { font-size:.65rem; color:#1a237e; margin-top:14px; word-break:break-all;
               background:#e8eaf6; border-radius:6px; padding:6px 10px; }
      </style></head><body>
      <div class="card">
        <h2>Our Digital Menu</h2>
        <p>Scan to browse &amp; order</p>
        <img src="${dataUrl}" />
        <div class="url">${menuUrl}</div>
      </div></body></html>`);
    win.document.close(); win.focus();
    setTimeout(() => win.print(), 300);
  }
}

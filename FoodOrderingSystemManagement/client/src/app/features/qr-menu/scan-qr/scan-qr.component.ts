import {
  Component, OnInit, OnDestroy, inject, signal, ViewChild, ElementRef, AfterViewInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { BrowserQRCodeReader, IScannerControls } from '@zxing/browser';

export interface ScanResult {
  text: string;
  tableNumber: number | null;
  isMenuUrl: boolean;
  scannedAt: Date;
}

@Component({
  selector: 'app-scan-qr',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatIconModule, MatButtonModule,
    MatFormFieldModule, MatInputModule, MatSelectModule,
    MatProgressSpinnerModule, MatSnackBarModule,
  ],
  templateUrl: './scan-qr.component.html',
  styleUrl: './scan-qr.component.scss',
})
export class ScanQrComponent implements OnInit, AfterViewInit, OnDestroy {
  private snack = inject(MatSnackBar);

  @ViewChild('videoEl') videoEl!: ElementRef<HTMLVideoElement>;

  private reader   = new BrowserQRCodeReader();
  private controls: IScannerControls | null = null;

  // Signals
  scanning          = signal(false);
  cameraError       = signal<string | null>(null);
  devices           = signal<MediaDeviceInfo[]>([]);
  selectedDeviceId  = signal<string | null>(null);
  latestResult      = signal<ScanResult | null>(null);
  history           = signal<ScanResult[]>([]);

  // Manual fallback
  manualUrl = '';

  async ngOnInit(): Promise<void> {
    try {
      const devs = await BrowserQRCodeReader.listVideoInputDevices();
      this.devices.set(devs);
      if (devs.length > 0) {
        this.selectedDeviceId.set(devs[0].deviceId);
      }
    } catch {
      this.cameraError.set('Could not access camera. Please allow camera permissions or use manual entry below.');
    }
  }

  ngAfterViewInit(): void {}

  async startScan(): Promise<void> {
    if (this.scanning()) return;
    this.cameraError.set(null);
    this.scanning.set(true);
    try {
      this.controls = await this.reader.decodeFromVideoDevice(
        this.selectedDeviceId() ?? undefined,
        this.videoEl.nativeElement,
        (result, err) => {
          if (result) {
            const text = result.getText();
            const parsed = this.parseUrl(text);
            this.latestResult.set(parsed);
            this.history.update(h => [parsed, ...h.slice(0, 9)]);
          }
        }
      );
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : 'Camera error';
      this.cameraError.set(`Camera error: ${msg}`);
      this.scanning.set(false);
    }
  }

  stopScan(): void {
    this.controls?.stop();
    this.controls = null;
    this.scanning.set(false);
  }

  onDeviceChange(deviceId: string): void {
    this.selectedDeviceId.set(deviceId);
    if (this.scanning()) {
      this.stopScan();
      setTimeout(() => this.startScan(), 200);
    }
  }

  parseManual(): void {
    const url = this.manualUrl.trim();
    if (!url) return;
    const result = this.parseUrl(url);
    this.latestResult.set(result);
    this.history.update(h => [result, ...h.slice(0, 9)]);
  }

  parseUrl(text: string): ScanResult {
    let tableNumber: number | null = null;
    let isMenuUrl = false;
    try {
      const url = new URL(text);
      const t = url.searchParams.get('table');
      if (t) {
        tableNumber = parseInt(t, 10);
        isMenuUrl = url.pathname.includes('menu');
      }
    } catch {}
    return { text, tableNumber, isMenuUrl, scannedAt: new Date() };
  }

  openUrl(url: string): void {
    window.open(url, '_blank');
  }

  copyText(text: string): void {
    navigator.clipboard.writeText(text).then(() =>
      this.snack.open('Copied!', '', { duration: 1500 })
    );
  }

  clearHistory(): void {
    this.history.set([]);
    this.latestResult.set(null);
  }

  ngOnDestroy(): void {
    this.stopScan();
  }
}

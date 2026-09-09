import {
  Component, Output, EventEmitter, OnDestroy, signal, inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormsModule } from '@angular/forms';
import { NotificationService } from '../../services/notification.service';

// Loaded lazily to avoid SSR issues
let BrowserMultiFormatReader: any;
let DecodeHintType: any;
let BarcodeFormat: any;

@Component({
  selector: 'app-barcode-scanner',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatSelectModule, MatFormFieldModule,
  ],
  template: `
    <div class="scanner-wrap">
      @if (!active()) {
        <button mat-stroked-button (click)="start()" class="cam-btn">
          <mat-icon>photo_camera</mat-icon> Scan with Camera
        </button>
      } @else {
        <div class="cam-box">
          <video #videoEl id="barcode-video" autoplay muted playsinline class="cam-feed"></video>
          <div class="scan-overlay">
            <div class="scan-frame">
              <div class="scan-line"><div class="scan-beam"></div></div>
            </div>
          </div>

          @if (cameras().length > 1) {
            <mat-form-field appearance="outline" class="cam-select">
              <mat-label>Camera</mat-label>
              <mat-select [(ngModel)]="selectedCamera" (ngModelChange)="switchCamera($event)">
                @for (cam of cameras(); track cam.deviceId) {
                  <mat-option [value]="cam.deviceId">{{ cam.label || 'Camera ' + $index }}</mat-option>
                }
              </mat-select>
            </mat-form-field>
          }

          @if (loading()) {
            <div class="cam-loading"><mat-spinner diameter="32" /></div>
          }

          <button mat-icon-button class="stop-btn" (click)="stop()">
            <mat-icon>close</mat-icon>
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    .scanner-wrap { display: flex; flex-direction: column; align-items: flex-start; gap: 8px; }

    .cam-btn { gap: 6px; }

    .cam-box {
      position: relative;
      width: 100%;
      max-width: 380px;
      border-radius: 12px;
      overflow: hidden;
      background: #000;
      border: 2px solid var(--primary, #ff5722);
    }

    .cam-feed { width: 100%; display: block; aspect-ratio: 4/3; object-fit: cover; }

    .scan-overlay {
      position: absolute; inset: 0;
      display: flex; align-items: center; justify-content: center;
      pointer-events: none;
    }
    .scan-frame {
      width: 72%; height: 38%;
      border: 2px solid rgba(255,255,255,.7);
      border-radius: 6px;
      position: relative;
      overflow: hidden;
      box-shadow: 0 0 0 2000px rgba(0,0,0,.35);
    }
    .scan-frame::before, .scan-frame::after,
    .scan-frame > .scan-line::before, .scan-frame > .scan-line::after {
      content: ''; position: absolute; width: 18px; height: 18px;
      border-color: var(--primary, #ff5722); border-style: solid;
    }
    .scan-frame::before  { top: -2px;  left: -2px;  border-width: 3px 0 0 3px; }
    .scan-frame::after   { top: -2px;  right: -2px; border-width: 3px 3px 0 0; }
    .scan-line {
      position: absolute; inset: 0;
    }
    .scan-line::before { bottom: -2px; left: -2px;  border-width: 0 0 3px 3px; }
    .scan-line::after  { bottom: -2px; right: -2px; border-width: 0 3px 3px 0; }
    .scan-line::before, .scan-line::after { content: ''; position: absolute; width: 18px; height: 18px; border-color: var(--primary, #ff5722); border-style: solid; }
    .scan-beam {
      position: absolute; left: 0; right: 0; height: 2px;
      background: rgba(255,87,34,.9);
      box-shadow: 0 0 8px 2px rgba(255,87,34,.6);
      animation: scanAnim 1.8s ease-in-out infinite;
    }
    @keyframes scanAnim {
      0%   { top: 10%; }
      50%  { top: 85%; }
      100% { top: 10%; }
    }

    .cam-select {
      position: absolute; bottom: 6px; left: 6px; right: 6px;
      background: rgba(0,0,0,.6); border-radius: 6px;
    }

    .cam-loading {
      position: absolute; inset: 0;
      display: flex; align-items: center; justify-content: center;
      background: rgba(0,0,0,.4);
    }

    .stop-btn {
      position: absolute; top: 4px; right: 4px;
      background: rgba(0,0,0,.55); color: #fff;
    }
  `],
})
export class BarcodeScannerComponent implements OnDestroy {
  @Output() scanned = new EventEmitter<string>();

  private notify  = inject(NotificationService);
  private reader: any = null;
  private controls: any = null;

  private static readonly CAM_KEY = 'preferred_scan_camera';

  active   = signal(false);
  loading  = signal(false);
  cameras  = signal<MediaDeviceInfo[]>([]);
  selectedCamera = '';

  async start(): Promise<void> {
    this.active.set(true);
    this.loading.set(true);
    try {
      if (!BrowserMultiFormatReader) {
        const browser = await import('@zxing/browser');
        const library = await import('@zxing/library');
        BrowserMultiFormatReader = browser.BrowserMultiFormatReader;
        DecodeHintType  = library.DecodeHintType;
        BarcodeFormat   = library.BarcodeFormat;
      }

      // TRY_HARDER makes ZXing work harder on each frame — essential for DroidCam
      const hints = new Map();
      hints.set(DecodeHintType.TRY_HARDER, true);
      hints.set(DecodeHintType.POSSIBLE_FORMATS, [
        BarcodeFormat.CODE_128,
        BarcodeFormat.QR_CODE,
        BarcodeFormat.EAN_13,
        BarcodeFormat.EAN_8,
        BarcodeFormat.DATA_MATRIX,
        BarcodeFormat.CODE_39,
      ]);
      this.reader = new BrowserMultiFormatReader(hints);

      const devices: MediaDeviceInfo[] = await BrowserMultiFormatReader.listVideoInputDevices();
      this.cameras.set(devices);

      const saved = localStorage.getItem(BarcodeScannerComponent.CAM_KEY);
      const preferred = devices.find(d => d.deviceId === saved) ?? devices[0];
      this.selectedCamera = preferred?.deviceId ?? '';
      if (this.selectedCamera) {
        localStorage.setItem(BarcodeScannerComponent.CAM_KEY, this.selectedCamera);
      }

      await this.startDecode(this.selectedCamera);
    } catch (err) {
      this.notify.error('Camera access denied or unavailable.');
      this.active.set(false);
    } finally {
      this.loading.set(false);
    }
  }

  async switchCamera(deviceId: string): Promise<void> {
    localStorage.setItem(BarcodeScannerComponent.CAM_KEY, deviceId);
    if (this.controls) { this.controls.stop(); this.controls = null; }
    await this.startDecode(deviceId);
  }

  private async startDecode(deviceId: string): Promise<void> {
    const videoEl = document.getElementById('barcode-video') as HTMLVideoElement;
    if (!videoEl || !this.reader) return;

    this.controls = await this.reader.decodeFromVideoDevice(
      deviceId || undefined,
      videoEl,
      (result: any, err: any) => {
        if (result) {
          const code = result.getText();
          this.scanned.emit(code);
          this.stop();
        }
      }
    );
  }

  stop(): void {
    if (this.controls) { this.controls.stop(); this.controls = null; }
    this.active.set(false);
  }

  ngOnDestroy(): void { this.stop(); }
}

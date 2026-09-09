import { Injectable, OnDestroy } from '@angular/core';
import { Subject, fromEvent, Subscription } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class BarcodeService implements OnDestroy {
  private buffer = '';
  private lastKeyTime = 0;
  private readonly THRESHOLD_MS = 80;  // USB scanners type < 80 ms between chars
  private readonly MIN_LEN = 4;

  private readonly scanSubject = new Subject<string>();
  /** Emits every barcode scanned from a USB/Bluetooth scanner (keyboard-wedge mode). */
  readonly scan$ = this.scanSubject.asObservable();

  private sub: Subscription;

  constructor() {
    this.sub = fromEvent<KeyboardEvent>(document, 'keydown').subscribe(e =>
      this.handleKey(e)
    );
  }

  private handleKey(e: KeyboardEvent): void {
    const target = e.target as HTMLElement;
    const tag = target.tagName;
    // Skip normal form inputs unless they explicitly opt-in via data-barcode-input attribute
    if ((tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') &&
        !(target as HTMLInputElement).dataset['barcodeInput']) {
      this.buffer = '';
      return;
    }

    const now = Date.now();

    if (e.key === 'Enter') {
      if (this.buffer.length >= this.MIN_LEN) {
        this.scanSubject.next(this.buffer.trim());
      }
      this.buffer = '';
      return;
    }

    if (e.key.length === 1) {
      if (now - this.lastKeyTime < this.THRESHOLD_MS) {
        this.buffer += e.key;
      } else {
        this.buffer = e.key;
      }
    }
    this.lastKeyTime = now;
  }

  ngOnDestroy(): void { this.sub.unsubscribe(); }
}

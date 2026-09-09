import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { NotificationService } from '../../shared/services/notification.service';

export type PrinterStatus = 'connecting' | 'connected' | 'disconnected';

const RECEIPT_PRINTER_KEY = 'printer_receipt_name';
const LABEL_PRINTER_KEY   = 'printer_label_name';
const POLL_INTERVAL_MS    = 4000;
const AGENT_URL           = 'http://localhost:9100';

@Injectable({ providedIn: 'root' })
export class PrinterService {
  private notify = inject(NotificationService);
  private http = inject(HttpClient);

  private pollHandle: ReturnType<typeof setInterval> | null = null;
  private connecting = false;

  readonly status   = signal<PrinterStatus>('disconnected');
  readonly printers = signal<string[]>([]);

  async connect(): Promise<void> {
    if (this.connecting || this.status() === 'connected') return;
    this.connecting = true;
    this.status.set('connecting');

    try {
      await firstValueFrom(this.http.get(`${AGENT_URL}/health`));
      this.status.set('connected');
      this.startPolling();
    } catch (err) {
      console.error('Print agent not reachable (is FoodOrder.PrintAgent running on port 9100?):', err);
      this.status.set('disconnected');
    } finally {
      this.connecting = false;
    }
  }

  private startPolling(): void {
    this.stopPolling();
    this.refreshPrinters();
    this.pollHandle = setInterval(() => this.refreshPrinters(), POLL_INTERVAL_MS);
  }

  private stopPolling(): void {
    if (this.pollHandle) {
      clearInterval(this.pollHandle);
      this.pollHandle = null;
    }
  }

  private async refreshPrinters(): Promise<void> {
    try {
      const found = await firstValueFrom(this.http.get<string[]>(`${AGENT_URL}/printers`));
      const previous = new Set(this.printers());
      const current  = new Set(found);

      for (const name of found) {
        if (!previous.has(name)) this.notify.success(`Printer connected: ${name}`);
      }
      for (const name of previous) {
        if (!current.has(name)) this.notify.warn(`Printer disconnected: ${name}`);
      }

      this.printers.set(found);
      this.status.set('connected');
    } catch (err) {
      console.error('Failed to list printers (print agent may have stopped):', err);
      this.status.set('disconnected');
      this.printers.set([]);
      this.stopPolling();
    }
  }

  async printRaw(printerName: string, data: string, format: 'plain' | 'hex' = 'plain'): Promise<boolean> {
    if (this.status() !== 'connected') {
      this.notify.error('Printer not connected. Falling back to browser print.');
      return false;
    }
    try {
      const res = await firstValueFrom(
        this.http.post<{ success: boolean; error?: string }>(`${AGENT_URL}/print`, {
          printerName,
          data,
          format,
        })
      );
      if (!res.success) throw new Error(res.error ?? 'Unknown print agent error');
      return true;
    } catch (err) {
      console.error('Print failed:', err);
      this.notify.error('Print failed — falling back to browser print.');
      return false;
    }
  }

  async printReceipt(escposData: string): Promise<boolean> {
    const printer = this.getReceiptPrinter();
    if (!printer) {
      this.notify.error('No receipt printer selected. Open the printer menu and set one under "Receipt".');
      return false;
    }
    return this.printRaw(printer, escposData);
  }

  async printLabel(zplData: string): Promise<boolean> {
    const printer = this.getLabelPrinter();
    if (!printer) {
      this.notify.error('No label printer selected. Open the printer menu and set one under "Label".');
      return false;
    }
    return this.printRaw(printer, zplData);
  }

  getReceiptPrinter(): string | null { return localStorage.getItem(RECEIPT_PRINTER_KEY); }
  setReceiptPrinter(name: string): void {
    localStorage.setItem(RECEIPT_PRINTER_KEY, name);
    this.notify.success(`"${name}" set as receipt printer.`);
  }

  getLabelPrinter(): string | null { return localStorage.getItem(LABEL_PRINTER_KEY); }
  setLabelPrinter(name: string): void {
    localStorage.setItem(LABEL_PRINTER_KEY, name);
    this.notify.success(`"${name}" set as label printer.`);
  }
}

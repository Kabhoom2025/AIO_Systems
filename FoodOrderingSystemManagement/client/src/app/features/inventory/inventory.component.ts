import { Component, inject, OnInit, OnDestroy, AfterViewInit, signal, computed, ElementRef, ViewChild } from '@angular/core';
import { Subscription, forkJoin } from 'rxjs';
import * as QRCode from 'qrcode';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatRippleModule } from '@angular/material/core';
import { MatDialog } from '@angular/material/dialog';
import { InventoryService } from '../../core/services/inventory.service';
import { NotificationService } from '../../shared/services/notification.service';
import { ConfirmationDialogComponent } from '../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { InventoryFormDialogComponent } from './inventory-form-dialog.component';
import { StockAdjustDialogComponent } from './stock-adjust-dialog.component';
import { PurchaseStockDialogComponent } from './purchase-stock-dialog.component';
import { WasteManagementDialogComponent } from './waste-management-dialog.component';
import { StockTransferDialogComponent } from './stock-transfer-dialog.component';
import { InventoryAuditDialogComponent } from './inventory-audit-dialog.component';
import { InventoryItem } from '../../core/models/inventory.model';
import { BarcodeService } from '../../core/services/barcode.service';
import { BarcodeScannerComponent } from '../../shared/components/barcode-scanner/barcode-scanner.component';

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatChipsModule,
    MatRippleModule,
    BarcodeScannerComponent,
  ],
  templateUrl: './inventory.component.html',
  styleUrl: './inventory.component.scss',
})
export class InventoryComponent implements OnInit, AfterViewInit, OnDestroy {
  private inventoryService = inject(InventoryService);
  private barcodeService   = inject(BarcodeService);
  private dialog           = inject(MatDialog);
  private notify           = inject(NotificationService);

  private barcodeSub: Subscription | null = null;

  @ViewChild('qrCanvas') qrCanvas!: ElementRef<HTMLCanvasElement>;

  items         = signal<InventoryItem[]>([]);
  loading       = signal(false);
  autoFilling   = signal(false);
  mobilePanel   = signal(false);
  appUrl        = window.location.origin;
  searchTerm    = signal('');
  filterLow     = signal(false);
  filterExpiring = signal(false);
  viewMode      = signal<'list' | 'cards'>('list');
  highlightedId = signal<number | null>(null);

  filtered = computed(() => {
    const term = this.searchTerm().toLowerCase();
    const today = new Date();
    const soon  = new Date(); soon.setDate(today.getDate() + 30);
    return this.items().filter(i => {
      const matchesSearch = i.name.toLowerCase().includes(term) ||
        i.unit.toLowerCase().includes(term) ||
        (i.description ?? '').toLowerCase().includes(term);
      const matchesLow = !this.filterLow() || i.isLowStock;
      const matchesExp = !this.filterExpiring() ||
        (i.expiryDate != null && new Date(i.expiryDate) <= soon);
      return matchesSearch && matchesLow && matchesExp;
    });
  });

  totalItems    = computed(() => this.items().length);
  lowStockCount = computed(() => this.items().filter(i => i.isLowStock).length);
  expiringCount = computed(() => {
    const soon = new Date(); soon.setDate(soon.getDate() + 30);
    return this.items().filter(i => i.expiryDate != null && new Date(i.expiryDate) <= soon).length;
  });

  ngAfterViewInit(): void {
    if (this.mobilePanel()) this.renderQr();
  }

  ngOnInit(): void {
    this.load();
    this.barcodeSub = this.barcodeService.scan$.subscribe(code => this.onBarcodeScanned(code));
  }

  ngOnDestroy(): void { this.barcodeSub?.unsubscribe(); }

  onBarcodeScanned(code: string): void {
    const sep = code.lastIndexOf(':');
    if (sep > 0) {
      const barcode = code.substring(0, sep);
      const qty     = parseFloat(code.substring(sep + 1));
      if (!isNaN(qty) && qty !== 0) {
        this.autoSaveStock(barcode, qty);
        return;
      }
    }
    const existing = this.items().find(i => i.barcode === code);
    if (existing) { this.openAdjustFromScan(existing); return; }
    this.inventoryService.getByBarcode(code).subscribe({
      next: res => {
        if (res.data) {
          if (!this.items().find(i => i.id === res.data.id)) {
            this.items.update(list => [...list, res.data]);
          }
          this.openAdjustFromScan(res.data);
        }
      },
      error: () => this.notify.error(`No inventory item found for barcode: ${code}`),
    });
  }

  private autoSaveStock(barcode: string, quantity: number): void {
    const item = this.items().find(i => i.barcode === barcode);
    if (!item) { this.notify.error(`No item found for barcode: ${barcode}`); return; }
    this.inventoryService.adjustStock(item.id, { quantity, note: 'Auto-added via barcode scan' }).subscribe({
      next: (res) => {
        this.items.update(list => list.map(i => i.id === item.id ? res.data : i));
        this.highlightItem(item.id);
        this.notify.success(`Auto-saved: +${quantity} ${res.data.unit} to ${res.data.name}`);
      },
      error: (err) => this.notify.error(err?.error?.message || 'Auto stock update failed.'),
    });
  }

  private openAdjustFromScan(item: InventoryItem): void {
    this.highlightItem(item.id);
    const ref = this.dialog.open(StockAdjustDialogComponent, {
      data: { item, fromScan: true }, width: '400px', disableClose: true,
    });
    ref.afterClosed().subscribe(result => {
      if (!result) return;
      this.inventoryService.adjustStock(item.id, result).subscribe({
        next: (res) => {
          this.notify.success(`Stock updated — ${res.data.name}: ${res.data.currentStock} ${res.data.unit}`);
          this.items.update(list => list.map(i => i.id === item.id ? res.data : i));
          this.highlightItem(item.id);
        },
        error: (err) => this.notify.error(err?.error?.message || 'Stock adjust failed.'),
      });
    });
  }

  private highlightItem(id: number): void {
    this.highlightedId.set(id);
    this.filterLow.set(false);
    this.filterExpiring.set(false);
    this.searchTerm.set('');
    setTimeout(() => {
      document.getElementById(`inv-item-${id}`)?.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }, 50);
    setTimeout(() => this.highlightedId.set(null), 3000);
  }

  load(): void {
    this.loading.set(true);
    this.inventoryService.getAll().subscribe({
      next: (res) => { this.items.set(res.data ?? []); this.loading.set(false); },
      error: () => { this.notify.error('Failed to load inventory.'); this.loading.set(false); },
    });
  }

  openCreate(): void {
    const ref = this.dialog.open(InventoryFormDialogComponent, {
      data: {}, width: '480px', disableClose: true,
    });
    ref.afterClosed().subscribe(result => {
      if (!result) return;
      this.inventoryService.create(result).subscribe({
        next: (res) => { this.notify.success('Item created.'); this.items.update(list => [...list, res.data]); },
        error: (err) => this.notify.error(err?.error?.message || 'Create failed.'),
      });
    });
  }

  openEdit(item: InventoryItem): void {
    const ref = this.dialog.open(InventoryFormDialogComponent, {
      data: { item }, width: '480px', disableClose: true,
    });
    ref.afterClosed().subscribe(result => {
      if (!result) return;
      this.inventoryService.update(item.id, result).subscribe({
        next: (res) => {
          this.notify.success('Item updated.');
          this.items.update(list => list.map(i => i.id === item.id ? res.data : i));
        },
        error: (err) => this.notify.error(err?.error?.message || 'Update failed.'),
      });
    });
  }

  openAdjust(item: InventoryItem): void {
    const ref = this.dialog.open(StockAdjustDialogComponent, {
      data: { item }, width: '400px', disableClose: true,
    });
    ref.afterClosed().subscribe(result => {
      if (!result) return;
      this.inventoryService.adjustStock(item.id, result).subscribe({
        next: (res) => {
          this.notify.success('Stock adjusted.');
          this.items.update(list => list.map(i => i.id === item.id ? res.data : i));
        },
        error: (err) => this.notify.error(err?.error?.message || 'Adjust failed.'),
      });
    });
  }

  openPurchase(item: InventoryItem): void {
    const ref = this.dialog.open(PurchaseStockDialogComponent, {
      data: item, width: '540px', disableClose: true,
    });
    ref.afterClosed().subscribe(result => {
      if (!result) return;
      this.inventoryService.purchaseStock(item.id, result).subscribe({
        next: (res) => {
          this.notify.success(`Purchase recorded. ${res.data.name}: ${res.data.currentStock} ${res.data.unit}`);
          this.items.update(list => list.map(i => i.id === item.id ? res.data : i));
          this.highlightItem(item.id);
        },
        error: (err) => this.notify.error(err?.error?.message || 'Purchase failed.'),
      });
    });
  }

  openWaste(item: InventoryItem): void {
    const ref = this.dialog.open(WasteManagementDialogComponent, {
      data: item, width: '540px', disableClose: true,
    });
    ref.afterClosed().subscribe(result => {
      if (!result) return;
      this.inventoryService.wasteStock(item.id, result).subscribe({
        next: (res) => {
          this.notify.success(`Waste recorded. ${res.data.name}: ${res.data.currentStock} ${res.data.unit}`);
          this.items.update(list => list.map(i => i.id === item.id ? res.data : i));
          this.highlightItem(item.id);
        },
        error: (err) => this.notify.error(err?.error?.message || 'Waste record failed.'),
      });
    });
  }

  openTransfer(item: InventoryItem): void {
    const ref = this.dialog.open(StockTransferDialogComponent, {
      data: { sourceItem: item, allItems: this.items() }, width: '540px', disableClose: true,
    });
    ref.afterClosed().subscribe(result => {
      if (!result) return;
      this.inventoryService.transferStock(item.id, result).subscribe({
        next: (res) => {
          const { source, target } = res.data;
          this.items.update(list => list.map(i => {
            if (i.id === source.id) return source;
            if (i.id === target.id) return target;
            return i;
          }));
          this.notify.success(`Transferred ${result.quantity} ${item.unit} from ${source.name} to ${target.name}.`);
        },
        error: (err) => this.notify.error(err?.error?.message || 'Transfer failed.'),
      });
    });
  }

  openAudit(item: InventoryItem): void {
    this.dialog.open(InventoryAuditDialogComponent, {
      data: item, width: '640px',
    });
  }

  confirmDelete(item: InventoryItem): void {
    const ref = this.dialog.open(ConfirmationDialogComponent, {
      data: {
        title: 'Delete Inventory Item',
        message: `Delete "${item.name}"? This cannot be undone.`,
        confirmText: 'Delete',
        danger: true,
      },
      width: '400px',
    });
    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.inventoryService.delete(item.id).subscribe({
        next: () => { this.notify.success('Item deleted.'); this.items.update(list => list.filter(i => i.id !== item.id)); },
        error: (err) => this.notify.error(err?.error?.message || 'Delete failed.'),
      });
    });
  }

  toggleLowFilter(): void   { this.filterLow.update(v => !v); if (this.filterLow()) this.filterExpiring.set(false); }
  toggleExpFilter(): void   { this.filterExpiring.update(v => !v); if (this.filterExpiring()) this.filterLow.set(false); }
  setView(mode: 'list' | 'cards'): void { this.viewMode.set(mode); }

  expiryStatus(item: InventoryItem): 'expired' | 'expiring' | 'ok' | 'none' {
    if (!item.expiryDate) return 'none';
    const d = new Date(item.expiryDate);
    const now = new Date();
    const soon = new Date(); soon.setDate(now.getDate() + 30);
    if (d < now) return 'expired';
    if (d <= soon) return 'expiring';
    return 'ok';
  }

  stockBarWidth(item: InventoryItem): number {
    if (item.minimumStock === 0) return 100;
    const ratio = item.currentStock / (item.minimumStock * 3);
    return Math.min(100, Math.round(ratio * 100));
  }

  stockBarColor(item: InventoryItem): string {
    if (item.isLowStock) return '#f44336';
    if (item.currentStock <= item.minimumStock * 1.5) return '#ff9800';
    return '#4caf50';
  }

  autoFillBarcodes(): void {
    const missing = this.items().filter(i => !i.barcode);
    if (missing.length === 0) { this.notify.success('All items already have barcodes.'); return; }
    this.autoFilling.set(true);
    const requests = missing.map(item => {
      const barcode = item.name.toUpperCase().replace(/[^A-Z0-9]/g, '');
      return this.inventoryService.update(item.id, {
        name: item.name, description: item.description ?? '', barcode,
        unit: item.unit, currentStock: item.currentStock, minimumStock: item.minimumStock,
        isActive: item.isActive, expiryDate: item.expiryDate,
      });
    });
    forkJoin(requests).subscribe({
      next: (results) => {
        const updated = new Map(results.map(r => [r.data.id, r.data]));
        this.items.update(list => list.map(i => updated.get(i.id) ?? i));
        this.autoFilling.set(false);
        this.notify.success(`Barcodes filled for ${results.length} item${results.length !== 1 ? 's' : ''}.`);
      },
      error: (err) => {
        this.autoFilling.set(false);
        this.notify.error(err?.error?.message || 'Auto-fill failed.');
      },
    });
  }

  downloadBarcodes(): void {
    const items = this.items();
    const win = window.open('', '_blank');
    if (!win) { this.notify.error('Pop-up blocked — please allow pop-ups and try again.'); return; }
    win.document.write(this.buildBarcodeHtml(items));
    win.document.close();
  }

  private buildBarcodeHtml(items: InventoryItem[]): string {
    const rows = items.map((item, idx) => {
      const bc = item.barcode || item.name.toUpperCase().replace(/[^A-Z0-9]/g, '');
      return `<div class="bc-card"><canvas id="bc-${idx}" class="bc-canvas"></canvas>`
        + `<p class="bc-name">${this.esc(item.name)}</p>`
        + `<p class="bc-val">${this.esc(bc)}</p></div>`;
    }).join('');

    const draws = items.map((item, idx) => {
      const bc = item.barcode || item.name.toUpperCase().replace(/[^A-Z0-9]/g, '');
      return `try{drawCode128(document.getElementById('bc-${idx}'),${JSON.stringify(bc)},2);}catch(e){document.getElementById('bc-${idx}').insertAdjacentHTML('afterend','<span class=err>'+e.message+'</span>');}`;
    }).join('\n');

    return `<!DOCTYPE html><html><head>
<meta charset="utf-8"><title>Inventory Barcodes</title>
<style>
*{box-sizing:border-box;margin:0;padding:0}
body{font-family:system-ui,sans-serif;background:#f5f5f5;padding:24px}
h1{font-size:1rem;font-weight:700;margin-bottom:4px;color:#111}
.subtitle{font-size:.78rem;color:#6b7280;margin-bottom:16px}
.grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(190px,1fr));gap:12px}
.bc-card{background:#fff;border:1px solid #e5e7eb;border-radius:8px;padding:12px 10px 10px;display:flex;flex-direction:column;align-items:center;gap:5px}
.bc-canvas{max-width:100%;display:block;image-rendering:pixelated}
.bc-name{font-size:.78rem;font-weight:700;color:#111;text-align:center;margin-top:2px}
.bc-val{font-size:.66rem;color:#6b7280;letter-spacing:.08em;text-transform:uppercase;text-align:center;word-break:break-all}
.err{font-size:.7rem;color:#dc2626}
@media print{body{background:#fff;padding:8px}h1,h2{page-break-after:avoid}.bc-card{break-inside:avoid}}
</style></head><body>
<h1>Inventory Barcodes</h1>
<p class="subtitle">${items.length} item${items.length !== 1 ? 's' : ''} &mdash; Print this page to save as PDF</p>
<div class="grid">${rows}</div>
<script>
const RAW=["11011001100","11001101100","11001100110","10010011000","10010001100","10001001100","10011001000","10011000100","10001100100","11001001000","11001000100","11000100100","10110011100","10011011100","10011001110","10111001100","10011101100","10011100110","11001110010","11001011100","11001001110","11011100100","11001110100","11101101110","11101001100","11100101100","11100100110","11101100100","11100110100","11100110010","11011011000","11011000110","11000110110","10100011000","10001011000","10001000110","10110001000","10001101000","10001100010","11010001000","11000101000","11000100010","10110111000","10110001110","10001101110","10111011000","10111000110","10001110110","11101110110","11010001110","11000101110","11011101000","11011100010","11011101110","11101011000","11101000110","11100010110","11101101000","11101100010","11100011010","11101111010","11001000010","11110001010","10100110000","10100001100","10010110000","10010000110","10000101100","10000100110","10110010000","10110000100","10011010000","10011000010","10000110100","10000110010","11000010010","11001010000","11110111010","11000010100","10001111010","10100111100","10010111100","10010011110","10111100100","10011110100","10011110010","11110100100","11110010100","11110010010","11011011110","11011110110","11110110110","10101111000","10100011110","10001011110","10111101000","10111100010","11110101000","11110100010","10111011110","10111101110","11101011110","11110101110","11010000100","11010010000","11010011100"];
const STOP="1100011101011";
function bitsToWidths(b){const w=[];let c=1;for(let i=1;i<b.length;i++){if(b[i]===b[i-1])c++;else{w.push(c);c=1;}}w.push(c);return w;}
function drawCode128(canvas,text,bw){bw=bw||2;const H=56;const Q=bw*10;const codes=[104];for(const ch of text){const v=ch.charCodeAt(0)-32;if(v<0||v>94)throw new Error('Bad char: '+ch);codes.push(v);}let chk=104;for(let i=0;i<text.length;i++)chk+=(i+1)*(text.charCodeAt(i)-32);codes.push(chk%103);const allW=[];for(const c of codes)allW.push(...bitsToWidths(RAW[c]));allW.push(...bitsToWidths(STOP));const tot=allW.reduce((a,b)=>a+b,0);const pw=Q*2+tot*bw;const dpr=Math.min(window.devicePixelRatio||1,2);canvas.width=pw*dpr;canvas.height=H*dpr;canvas.style.width=pw+'px';canvas.style.height=H+'px';const ctx=canvas.getContext('2d');ctx.scale(dpr,dpr);ctx.fillStyle='#fff';ctx.fillRect(0,0,pw,H);ctx.fillStyle='#111';let x=Q;let bar=true;for(const w of allW){if(bar)ctx.fillRect(x,0,w*bw,H);x+=w*bw;bar=!bar;}}
${draws}
<\/script></body></html>`;
  }

  toggleMobilePanel(): void {
    this.mobilePanel.update(v => !v);
    if (this.mobilePanel()) setTimeout(() => this.renderQr(), 50);
  }

  private renderQr(): void {
    const canvas = this.qrCanvas?.nativeElement;
    if (!canvas) return;
    QRCode.toCanvas(canvas, this.appUrl, {
      width: 180, margin: 1,
      color: { dark: '#1b1b18', light: '#ffffff' },
    });
  }

  copyUrl(): void {
    navigator.clipboard.writeText(this.appUrl).then(
      () => this.notify.success('URL copied to clipboard.'),
      () => this.notify.error('Copy failed — select and copy manually.'),
    );
  }

  private esc(s: string): string {
    return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
  }
}

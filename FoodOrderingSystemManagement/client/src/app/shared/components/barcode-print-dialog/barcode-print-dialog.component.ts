import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { BarcodeDisplayComponent } from '../barcode-display/barcode-display.component';
import { PrinterService } from '../../../core/services/printer.service';
import { buildBarcodeZpl } from '../../../core/utils/zpl-builder';

export interface BarcodePrintDialogData {
  title: string;
  barcode: string;
}

@Component({
  selector: 'app-barcode-print-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, BarcodeDisplayComponent],
  template: `
    <div class="print-dialog">
      <div class="print-header">
        <mat-icon>barcode_reader</mat-icon>
        <h2>{{ data.title }}</h2>
      </div>

      <div class="print-body">
        <app-barcode-display [value]="data.barcode" [height]="90" [width]="2.5" />
        <p class="barcode-val">{{ data.barcode }}</p>
      </div>

      <div class="print-footer">
        <button mat-stroked-button mat-dialog-close>Close</button>
        <button mat-raised-button color="primary" (click)="print()">
          <mat-icon>print</mat-icon> Print
        </button>
      </div>
    </div>
  `,
  styles: [`
    .print-dialog { width: 360px; font-family: inherit; }

    .print-header {
      display: flex; align-items: center; gap: 10px;
      padding: 18px 24px 14px;
      border-bottom: 1px solid #f0f0f0;
      mat-icon { color: var(--primary, #ff5722); font-size: 26px; }
      h2 { margin: 0; font-size: 1rem; font-weight: 700; flex: 1; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    }

    .print-body {
      padding: 24px;
      display: flex; flex-direction: column; align-items: center; gap: 8px;
    }

    .barcode-val { margin: 0; font-size: .8rem; color: #9e9e9e; letter-spacing: 1px; }

    .print-footer {
      display: flex; justify-content: flex-end; gap: 10px;
      padding: 12px 24px 18px;
      border-top: 1px solid #f0f0f0;
    }

    @media print {
      .print-footer, .print-header mat-icon { display: none; }
      .print-dialog { width: auto; }
    }
  `],
})
export class BarcodePrintDialogComponent {
  private dialogRef = inject(MatDialogRef<BarcodePrintDialogComponent>);
  private printerService = inject(PrinterService);
  data: BarcodePrintDialogData = inject(MAT_DIALOG_DATA);

  async print(): Promise<void> {
    const zpl = buildBarcodeZpl(this.data.title, this.data.barcode);
    const printed = await this.printerService.printLabel(zpl);
    if (!printed) window.print();
  }
}

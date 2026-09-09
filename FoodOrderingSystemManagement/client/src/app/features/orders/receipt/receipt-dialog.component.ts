import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { Order } from '../../../core/models/order.model';
import { SettingsService } from '../../../core/services/settings.service';
import { PrinterService } from '../../../core/services/printer.service';
import { buildReceiptEscPos } from '../../../core/utils/escpos-builder';

@Component({
  selector: 'app-receipt-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, MatDividerModule],
  templateUrl: './receipt-dialog.component.html',
  styleUrl: './receipt-dialog.component.scss',
})
export class ReceiptDialogComponent {
  order: Order            = inject(MAT_DIALOG_DATA);
  private dialogRef       = inject(MatDialogRef<ReceiptDialogComponent>);
  readonly settingsService = inject(SettingsService);
  private printerService   = inject(PrinterService);

  settings = this.settingsService.settings;

  close(): void { this.dialogRef.close(); }

  async print(): Promise<void> {
    const escpos = buildReceiptEscPos(this.order, this.settings());
    const printed = await this.printerService.printReceipt(escpos);
    if (!printed) window.print();
  }
}

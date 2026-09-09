import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Table } from '../../../core/models/table.model';

export interface TransferDialogData {
  orderNumber: string;
  currentTableId: number | null;
  tables: Table[];
}

export interface TransferDialogResult {
  newTableId: number | null; // null = Takeaway
}

@Component({
  selector: 'app-transfer-table-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  template: `
    <h2 mat-dialog-title class="dialog-title">
      <mat-icon>swap_horiz</mat-icon>
      Transfer Table — {{ data.orderNumber }}
    </h2>

    <mat-dialog-content>
      <p class="hint">Select the table to move this order to:</p>
      <div class="chip-grid">
        <button class="t-chip"
          [class.active]="selected() === null"
          [class.current]="data.currentTableId === null"
          (click)="selected.set(null)">
          <mat-icon>takeout_dining</mat-icon>
          Takeaway
          @if (data.currentTableId === null) { <span class="cur-label">current</span> }
        </button>
      </div>
      @for (group of hallGroups; track group.hall) {
        <p class="hall-label" [class.hall-ac]="group.hall === 'AC'" [class.hall-nonac]="group.hall !== 'AC'">
          {{ group.hall }} Hall
        </p>
        <div class="chip-grid">
          @for (t of group.tables; track t.id) {
            <button class="t-chip"
              [class.active]="selected() === t.id"
              [class.current]="data.currentTableId === t.id"
              [class.occupied]="t.isOccupied && data.currentTableId !== t.id"
              (click)="selected.set(t.id)">
              <mat-icon>table_restaurant</mat-icon>
              T{{ t.tableNumber }}
              @if (data.currentTableId === t.id) { <span class="cur-label">current</span> }
              @else if (t.isOccupied) { <span class="occ-label">occupied</span> }
            </button>
          }
        </div>
      }
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-stroked-button (click)="close()">Cancel</button>
      <button mat-raised-button color="primary"
        [disabled]="selected() === undefined || selected() === data.currentTableId"
        (click)="confirm()">
        <mat-icon>check</mat-icon>
        Transfer
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-title { display: flex; align-items: center; gap: 10px; font-size: 1.1rem;
      mat-icon { color: #3f51b5; } }
    .hint { color: #757575; font-size: .88rem; margin: 0 0 14px; }
    .chip-grid { display: flex; flex-wrap: wrap; gap: 8px; padding-bottom: 4px; }
    .t-chip {
      display: flex; flex-direction: column; align-items: center; gap: 4px;
      padding: 10px 16px; border-radius: 10px; border: 2px solid #e0e0e0;
      background: #fff; cursor: pointer; transition: all .2s; color: #424242; min-width: 80px;
      mat-icon { font-size: 20px; width: 20px; height: 20px; color: #9e9e9e; }
      &:hover { border-color: #3f51b5; color: #3f51b5; mat-icon { color: #3f51b5; } }
      &.active { border-color: #3f51b5; background: #3f51b5; color: #fff;
        mat-icon { color: #fff; } }
      &.current { border-style: dashed; border-color: #9e9e9e; }
      &.occupied { border-color: #ffb74d; }
    }
    .cur-label { font-size: .65rem; background: #e8eaf6; color: #3f51b5;
      padding: 1px 5px; border-radius: 6px; font-weight: 700; }
    .occ-label { font-size: .65rem; background: #fff3e0; color: #e65100;
      padding: 1px 5px; border-radius: 6px; font-weight: 700; }
    .hall-label { font-size: .75rem; font-weight: 700; margin: 10px 0 6px;
      padding: 3px 12px; border-radius: 12px; display: inline-block;
      &.hall-ac    { background: #e3f2fd; color: #1565c0; }
      &.hall-nonac { background: #fff3e0; color: #e65100; } }
    mat-dialog-actions { gap: 8px; padding: 16px 24px !important; }
  `],
})
export class TransferTableDialogComponent {
  data: TransferDialogData = inject(MAT_DIALOG_DATA);
  private dialogRef = inject(MatDialogRef<TransferTableDialogComponent>);

  // undefined = nothing chosen yet; null = Takeaway; number = table id
  selected = signal<number | null | undefined>(undefined);

  get hallGroups(): { hall: string; tables: Table[] }[] {
    const map = new Map<string, Table[]>();
    for (const t of this.data.tables) {
      const h = t.hall ?? 'AC';
      if (!map.has(h)) map.set(h, []);
      map.get(h)!.push(t);
    }
    return ['AC', 'Non-AC'].filter(h => map.has(h)).map(h => ({ hall: h, tables: map.get(h)! }));
  }

  close(): void { this.dialogRef.close(null); }

  confirm(): void {
    this.dialogRef.close({ newTableId: this.selected() } as TransferDialogResult);
  }
}

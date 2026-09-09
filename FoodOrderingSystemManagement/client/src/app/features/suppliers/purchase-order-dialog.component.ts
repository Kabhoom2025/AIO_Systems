import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { InventoryItem } from '../../core/models/inventory.model';
import { Supplier } from '../../core/models/supplier.model';

export interface PurchaseOrderDialogData {
  supplier: Supplier;
  inventoryItems: InventoryItem[];
}

@Component({
  selector: 'app-purchase-order-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
            MatInputModule, MatButtonModule, MatIconModule, MatSelectModule, MatTooltipModule],
  template: `
    <div class="pod-wrap">
      <div class="pod-header">
        <mat-icon>receipt_long</mat-icon>
        <div>
          <span class="pod-title">New Purchase Order</span>
          <span class="pod-sub">{{ data.supplier.name }}</span>
        </div>
      </div>
      <form [formGroup]="form" class="pod-body" (ngSubmit)="submit()">
        <div class="pod-row">
          <mat-form-field appearance="outline">
            <mat-label>Expected Delivery</mat-label>
            <input matInput type="date" formControlName="expectedDate" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Notes</mat-label>
            <input matInput formControlName="notes" />
          </mat-form-field>
        </div>

        <div class="pod-items-head">
          <span>Order Items</span>
          <button type="button" class="btn-add-item" (click)="addItem()">
            <mat-icon>add</mat-icon> Add Item
          </button>
        </div>

        <div formArrayName="items" class="pod-items-list">
          @for (item of itemsArray.controls; track $index) {
            <div [formGroupName]="$index" class="pod-item-row">
              <mat-form-field appearance="outline" class="item-name-field">
                <mat-label>Item</mat-label>
                <mat-select formControlName="inventoryItemId" (selectionChange)="onItemSelect($index, $event.value)">
                  @for (inv of data.inventoryItems; track inv.id) {
                    <mat-option [value]="inv.id">{{ inv.name }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>
              <mat-form-field appearance="outline" class="item-qty-field">
                <mat-label>Qty</mat-label>
                <input matInput type="number" formControlName="quantity" min="0.01" step="0.01" (input)="recalc($index)" />
              </mat-form-field>
              <mat-form-field appearance="outline" class="item-price-field">
                <mat-label>Unit Price</mat-label>
                <input matInput type="number" formControlName="unitPrice" min="0" step="0.01" (input)="recalc($index)" />
                <span matPrefix>₹</span>
              </mat-form-field>
              <span class="item-total">₹{{ getItemTotal($index) | number:'1.2-2' }}</span>
              <button type="button" class="btn-remove" (click)="removeItem($index)" matTooltip="Remove">
                <mat-icon>close</mat-icon>
              </button>
            </div>
          }
        </div>

        <div class="pod-total-row">
          <span>Total</span>
          <span class="pod-total">₹{{ grandTotal() | number:'1.2-2' }}</span>
        </div>

        <div class="pod-actions">
          <button type="button" class="btn-cancel" (click)="cancel()">Cancel</button>
          <button type="submit" class="btn-save" [disabled]="form.invalid || itemsArray.length === 0">
            <mat-icon>receipt_long</mat-icon> Create PO
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .pod-wrap { width: 680px; font-family: inherit; }
    .pod-header { display: flex; align-items: center; gap: 12px; background: var(--primary); color: #fff; padding: 16px 20px; mat-icon { font-size: 24px; } }
    .pod-title { display: block; font-size: 1rem; font-weight: 700; }
    .pod-sub { display: block; font-size: .78rem; opacity: .8; }
    .pod-body { padding: 20px; display: flex; flex-direction: column; gap: 12px; max-height: 70vh; overflow-y: auto; }
    .pod-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    mat-form-field { width: 100%; }
    .pod-items-head { display: flex; align-items: center; justify-content: space-between; font-weight: 600; font-size: .88rem; color: #424242; }
    .btn-add-item { display: flex; align-items: center; gap: 4px; background: none; border: 1.5px solid var(--primary); color: var(--primary); border-radius: 7px; padding: 5px 12px; cursor: pointer; font-size: .82rem; font-weight: 600; &:hover { background: var(--primary); color: #fff; } mat-icon { font-size: 16px; } }
    .pod-items-list { display: flex; flex-direction: column; gap: 8px; }
    .pod-item-row { display: flex; align-items: center; gap: 8px; }
    .item-name-field { flex: 2; }
    .item-qty-field { flex: 1; }
    .item-price-field { flex: 1; }
    .item-total { min-width: 80px; text-align: right; font-weight: 700; color: #212121; font-size: .9rem; }
    .btn-remove { background: none; border: none; cursor: pointer; color: #bbb; display: flex; align-items: center; padding: 4px; border-radius: 6px; &:hover { color: #f44336; background: #ffebee; } mat-icon { font-size: 18px; } }
    .pod-total-row { display: flex; justify-content: space-between; align-items: center; padding: 10px 0; border-top: 2px solid #f0f0f0; font-weight: 600; color: #424242; }
    .pod-total { font-size: 1.2rem; font-weight: 700; color: var(--primary); }
    .pod-actions { display: flex; gap: 10px; justify-content: flex-end; padding-top: 4px; border-top: 1px solid #f0f0f0; }
    .btn-cancel { background: none; border: 1px solid #ddd; border-radius: 8px; padding: 0 18px; height: 38px; cursor: pointer; font-size: .87rem; color: #666; &:hover { background: #f5f5f5; } }
    .btn-save { display: flex; align-items: center; gap: 6px; background: var(--primary); color: #fff; border: none; border-radius: 8px; padding: 0 20px; height: 38px; font-size: .87rem; font-weight: 600; cursor: pointer; &:hover { opacity: .9; } &:disabled { opacity: .5; cursor: not-allowed; } mat-icon { font-size: 18px; } }
  `],
})
export class PurchaseOrderDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<PurchaseOrderDialogComponent>);
  readonly data = inject<PurchaseOrderDialogData>(MAT_DIALOG_DATA);

  form!: FormGroup;

  ngOnInit(): void {
    this.form = this.fb.group({
      expectedDate: [''],
      notes: [''],
      items: this.fb.array([]),
    });
    this.addItem();
  }

  get itemsArray(): FormArray { return this.form.get('items') as FormArray; }

  addItem(): void {
    this.itemsArray.push(this.fb.group({
      inventoryItemId: [null, Validators.required],
      itemName: [''],
      unit: [''],
      quantity: [1, [Validators.required, Validators.min(0.01)]],
      unitPrice: [0, [Validators.required, Validators.min(0)]],
    }));
  }

  removeItem(i: number): void { this.itemsArray.removeAt(i); }

  onItemSelect(index: number, id: number): void {
    const inv = this.data.inventoryItems.find(i => i.id === id);
    if (inv) {
      this.itemsArray.at(index).patchValue({ itemName: inv.name, unit: inv.unit });
    }
  }

  recalc(_: number): void {}

  getItemTotal(i: number): number {
    const row = this.itemsArray.at(i).value;
    return (row.quantity || 0) * (row.unitPrice || 0);
  }

  grandTotal(): number {
    return this.itemsArray.controls.reduce((sum, _, i) => sum + this.getItemTotal(i), 0);
  }

  submit(): void {
    if (this.form.invalid || this.itemsArray.length === 0) return;
    const val = this.form.value;
    this.dialogRef.close({
      supplierId: this.data.supplier.id,
      expectedDate: val.expectedDate || undefined,
      notes: val.notes || undefined,
      items: val.items.map((it: any) => ({
        inventoryItemId: it.inventoryItemId,
        itemName: it.itemName,
        unit: it.unit,
        quantity: +it.quantity,
        unitPrice: +it.unitPrice,
      })),
    });
  }

  cancel(): void { this.dialogRef.close(); }
}

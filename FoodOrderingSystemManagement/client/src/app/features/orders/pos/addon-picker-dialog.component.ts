import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AddOn } from '../../../core/models/addon.model';
import { FoodItem } from '../../../core/models/food-item.model';

export interface AddOnPickerData {
  item: FoodItem;
  addOns: AddOn[];
}

export interface AddOnPickerResult {
  selectedAddOns: AddOn[];
}

@Component({
  selector: 'app-addon-picker-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="ap-dialog">
      <div class="ap-header">
        <mat-icon>add_circle_outline</mat-icon>
        <div class="ap-title">
          <span class="ap-item-name">{{ data.item.itemName }}</span>
          <span class="ap-subtitle">Choose add-ons (optional)</span>
        </div>
      </div>

      <div class="ap-body">
        <div class="ap-base-row">
          <span>Base price</span>
          <span>₹{{ data.item.price | number:'1.2-2' }}</span>
        </div>

        <div class="ap-addon-list">
          @for (addon of availableAddOns(); track addon.id) {
            <button class="ap-addon-chip"
                    [class.selected]="isSelected(addon.id)"
                    (click)="toggle(addon)">
              <span class="ap-addon-name">{{ addon.name }}</span>
              <span class="ap-addon-price">+₹{{ addon.price | number:'1.2-2' }}</span>
              @if (isSelected(addon.id)) {
                <mat-icon class="ap-check">check_circle</mat-icon>
              }
            </button>
          }
        </div>

        <div class="ap-total-row">
          <span>Total per unit</span>
          <span class="ap-total">₹{{ totalPerUnit() | number:'1.2-2' }}</span>
        </div>
        @if (selectedAddOns().length > 0) {
          <div class="ap-selected-names">{{ selectedNames() }}</div>
        }
      </div>

      <div class="ap-actions">
        <button class="ap-btn-skip" (click)="addWithoutAddOns()">
          Skip add-ons
        </button>
        <button class="ap-btn-add" (click)="confirm()">
          <mat-icon>add_shopping_cart</mat-icon>
          Add to Cart
        </button>
      </div>
    </div>
  `,
  styles: [`
    .ap-dialog { width: 400px; font-family: inherit; }

    .ap-header {
      display: flex; align-items: flex-start; gap: 12px;
      background: var(--primary); color: #fff;
      padding: 16px 20px;
      mat-icon { font-size: 26px; margin-top: 2px; }
    }

    .ap-title { display: flex; flex-direction: column; gap: 2px; }
    .ap-item-name { font-size: 1rem; font-weight: 700; }
    .ap-subtitle { font-size: .78rem; opacity: .8; }

    .ap-body { padding: 16px 20px; }

    .ap-base-row {
      display: flex; justify-content: space-between;
      font-size: .88rem; color: #666; padding-bottom: 12px;
      border-bottom: 1px dashed #e0e0e0; margin-bottom: 14px;
    }

    .ap-addon-list {
      display: flex; flex-direction: column; gap: 8px; margin-bottom: 16px;
    }

    .ap-addon-chip {
      display: flex; align-items: center;
      border: 1.5px solid #e0e0e0; border-radius: 10px;
      background: #fafafa; padding: 10px 14px; cursor: pointer;
      transition: all .15s; text-align: left; width: 100%;
      &:hover { border-color: var(--primary); background: #fff5f2; }
      &.selected { border-color: var(--primary); background: #fff0eb; }
    }

    .ap-addon-name { flex: 1; font-size: .9rem; font-weight: 500; color: #333; }
    .ap-addon-price { font-size: .85rem; color: #e65100; font-weight: 600; margin-right: 8px; }
    .ap-check { font-size: 18px; color: var(--primary); }

    .ap-total-row {
      display: flex; justify-content: space-between; align-items: center;
      padding-top: 12px; border-top: 2px solid #e0e0e0;
      font-size: .95rem; font-weight: 700;
    }

    .ap-total { font-size: 1.15rem; color: var(--primary); font-weight: 800; }

    .ap-selected-names {
      text-align: right; font-size: .75rem; color: #888;
      margin-top: 4px;
    }

    .ap-actions {
      display: flex; gap: 10px; justify-content: flex-end;
      padding: 12px 20px; border-top: 1px solid #f0f0f0;
      background: #fafafa;
    }

    .ap-btn-skip {
      background: none; border: 1px solid #ddd; border-radius: 8px;
      padding: 0 16px; height: 38px; cursor: pointer; font-size: .85rem; color: #888;
      &:hover { background: #f5f5f5; }
    }

    .ap-btn-add {
      display: flex; align-items: center; gap: 7px;
      background: var(--primary); color: #fff; border: none; border-radius: 8px;
      padding: 0 20px; height: 38px; font-size: .87rem; font-weight: 600; cursor: pointer;
      mat-icon { font-size: 18px; }
      &:hover { opacity: .9; }
    }
  `],
})
export class AddOnPickerDialogComponent {
  readonly ref  = inject(MatDialogRef<AddOnPickerDialogComponent>);
  readonly data = inject<AddOnPickerData>(MAT_DIALOG_DATA);

  private _selected = signal<AddOn[]>([]);
  readonly selectedAddOns = this._selected.asReadonly();

  readonly availableAddOns = computed(() =>
    this.data.addOns.filter(a => a.isAvailable !== false)
  );

  readonly totalPerUnit = computed(() =>
    this.data.item.price + this._selected().reduce((s, a) => s + a.price, 0)
  );

  readonly selectedNames = computed(() =>
    this._selected().map(a => a.name).join(' · ')
  );

  isSelected(id: number): boolean {
    return this._selected().some(a => a.id === id);
  }

  toggle(addon: AddOn): void {
    this._selected.update(list =>
      list.some(a => a.id === addon.id)
        ? list.filter(a => a.id !== addon.id)
        : [...list, addon]
    );
  }

  confirm(): void {
    this.ref.close({ selectedAddOns: this._selected() } as AddOnPickerResult);
  }

  addWithoutAddOns(): void {
    this.ref.close({ selectedAddOns: [] } as AddOnPickerResult);
  }
}

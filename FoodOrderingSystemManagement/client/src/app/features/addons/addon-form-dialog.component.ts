import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CategoryService } from '../../core/services/category.service';
import { Category } from '../../core/models/category.model';
import { AddOn } from '../../core/models/addon.model';

export interface AddOnDialogData {
  addOn?: AddOn;
}

@Component({
  selector: 'app-addon-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSlideToggleModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <div class="afd-wrap">
      <div class="afd-header">
        <mat-icon>{{ isEdit ? 'edit' : 'add_circle_outline' }}</mat-icon>
        <span>{{ isEdit ? 'Edit Add-On' : 'New Add-On' }}</span>
      </div>

      <form [formGroup]="form" class="afd-body" (ngSubmit)="submit()">

        <mat-form-field appearance="outline">
          <mat-label>Name</mat-label>
          <input matInput formControlName="name" placeholder="e.g. Extra Cheese" />
          @if (form.get('name')!.hasError('required') && form.get('name')!.touched) {
            <mat-error>Name is required.</mat-error>
          }
          @if (form.get('name')!.hasError('maxlength')) {
            <mat-error>Max 100 characters.</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Price (₹)</mat-label>
          <input matInput type="number" formControlName="price" placeholder="0.00" min="0" step="0.50" />
          @if (form.get('price')!.hasError('required') && form.get('price')!.touched) {
            <mat-error>Price is required.</mat-error>
          }
          @if (form.get('price')!.hasError('min')) {
            <mat-error>Price cannot be negative.</mat-error>
          }
        </mat-form-field>

        <!-- Category multi-select -->
        <div class="afd-section-label">
          <mat-icon>category</mat-icon>
          <span>Applies to categories</span>
          @if (selectedCats().size > 0) {
            <span class="afd-sel-count">{{ selectedCats().size }} selected</span>
          }
        </div>
        <div class="afd-hint">This add-on appears in POS for items belonging to selected categories.</div>

        @if (loadingCats()) {
          <div class="afd-cats-loading">
            <mat-spinner diameter="24" />
            <span>Loading categories…</span>
          </div>
        } @else {
          <div class="afd-cat-grid">
            @for (cat of categories(); track cat.id) {
              <button type="button"
                      class="afd-cat-chip"
                      [class.selected]="isCatSelected(cat.categoryName)"
                      (click)="toggleCat(cat.categoryName)">
                <span class="afd-cat-emoji">{{ getCatEmoji(cat.categoryName) }}</span>
                <span class="afd-cat-name">{{ cat.categoryName }}</span>
                @if (isCatSelected(cat.categoryName)) {
                  <mat-icon class="afd-cat-check">check</mat-icon>
                }
              </button>
            }
          </div>
        }

        <div class="afd-toggle">
          <mat-slide-toggle formControlName="isAvailable" color="primary">Available</mat-slide-toggle>
          <span class="afd-toggle-hint">Unavailable add-ons are hidden in the POS.</span>
        </div>

        <div class="afd-actions">
          <button type="button" class="afd-btn-cancel" (click)="cancel()">Cancel</button>
          <button type="submit" class="afd-btn-save" [disabled]="form.invalid">
            <mat-icon>{{ isEdit ? 'save' : 'add' }}</mat-icon>
            {{ isEdit ? 'Save Changes' : 'Create' }}
          </button>
        </div>

      </form>
    </div>
  `,
  styles: [`
    .afd-wrap { width: 460px; font-family: inherit; }

    .afd-header {
      display: flex; align-items: center; gap: 10px;
      background: var(--primary); color: #fff;
      padding: 16px 20px; font-size: 1rem; font-weight: 700;
      mat-icon { font-size: 22px; }
    }

    .afd-body {
      padding: 20px; display: flex; flex-direction: column; gap: 6px;
      max-height: 80vh; overflow-y: auto;
    }

    mat-form-field { width: 100%; }

    .afd-section-label {
      display: flex; align-items: center; gap: 7px;
      font-size: .8rem; font-weight: 700; color: #424242;
      text-transform: uppercase; letter-spacing: .4px;
      margin-top: 6px;
      mat-icon { font-size: 16px; width: 16px; height: 16px; color: var(--primary); }
    }
    .afd-sel-count {
      margin-left: auto; background: var(--primary); color: #fff;
      font-size: .7rem; padding: 1px 8px; border-radius: 10px;
    }

    .afd-hint { font-size: .75rem; color: #9e9e9e; margin-bottom: 6px; }

    .afd-cats-loading {
      display: flex; align-items: center; gap: 10px;
      color: #9e9e9e; font-size: .85rem; padding: 12px 0;
    }

    .afd-cat-grid {
      display: grid; grid-template-columns: repeat(2, 1fr); gap: 8px;
      max-height: 220px; overflow-y: auto; padding: 2px;
    }

    .afd-cat-chip {
      display: flex; align-items: center; gap: 8px;
      border: 1.5px solid #e0e0e0; border-radius: 10px;
      background: #fafafa; padding: 9px 12px; cursor: pointer;
      transition: all .15s; text-align: left;
      &:hover { border-color: var(--primary); background: #fff5f2; }
      &.selected { border-color: var(--primary); background: #fff0eb; }
    }
    .afd-cat-emoji { font-size: 1.2rem; flex-shrink: 0; }
    .afd-cat-name  { flex: 1; font-size: .85rem; font-weight: 500; color: #333; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .afd-cat-check { font-size: 16px; color: var(--primary); flex-shrink: 0; }

    .afd-toggle {
      display: flex; align-items: center; gap: 14px; padding: 8px 0 4px;
    }
    .afd-toggle-hint { font-size: .75rem; color: #9e9e9e; }

    .afd-actions {
      display: flex; gap: 10px; justify-content: flex-end;
      padding-top: 12px; border-top: 1px solid #f0f0f0; margin-top: 8px;
    }

    .afd-btn-cancel {
      background: none; border: 1px solid #ddd; border-radius: 8px;
      padding: 0 18px; height: 38px; cursor: pointer; font-size: .87rem; color: #666;
      &:hover { background: #f5f5f5; }
    }
    .afd-btn-save {
      display: flex; align-items: center; gap: 6px;
      background: var(--primary); color: #fff; border: none; border-radius: 8px;
      padding: 0 20px; height: 38px; font-size: .87rem; font-weight: 600; cursor: pointer;
      mat-icon { font-size: 18px; }
      &:hover { opacity: .9; }
      &:disabled { opacity: .5; cursor: not-allowed; }
    }
  `],
})
export class AddOnFormDialogComponent implements OnInit {
  private fb             = inject(FormBuilder);
  private dialogRef      = inject(MatDialogRef<AddOnFormDialogComponent>);
  private categoryService = inject(CategoryService);
  readonly data          = inject<AddOnDialogData>(MAT_DIALOG_DATA);

  form!: FormGroup;
  isEdit      = false;
  categories  = signal<Category[]>([]);
  loadingCats = signal(false);
  selectedCats = signal<Set<string>>(new Set());

  ngOnInit(): void {
    this.isEdit = !!this.data?.addOn;

    this.form = this.fb.group({
      name:        [this.data?.addOn?.name        ?? '', [Validators.required, Validators.maxLength(100)]],
      price:       [this.data?.addOn?.price        ?? 0,  [Validators.required, Validators.min(0)]],
      isAvailable: [this.data?.addOn?.isAvailable  ?? true],
    });

    // Populate selected categories from existing category string (comma-separated)
    if (this.data?.addOn?.category) {
      const existing = new Set(
        this.data.addOn.category.split(',').map(c => c.trim()).filter(Boolean)
      );
      this.selectedCats.set(existing);
    }

    this.loadCategories();
  }

  private loadCategories(): void {
    this.loadingCats.set(true);
    this.categoryService.getAll().subscribe({
      next: (res) => {
        this.categories.set((res.data ?? []).filter(c => c.isActive));
        this.loadingCats.set(false);
      },
      error: () => this.loadingCats.set(false),
    });
  }

  isCatSelected(name: string): boolean {
    return this.selectedCats().has(name);
  }

  toggleCat(name: string): void {
    this.selectedCats.update(set => {
      const next = new Set(set);
      if (next.has(name)) next.delete(name);
      else next.add(name);
      return next;
    });
  }

  getCatEmoji(name: string): string {
    const n = name.toLowerCase();
    if (n.includes('pizza'))    return '🍕';
    if (n.includes('burger'))   return '🍔';
    if (n.includes('biryani'))  return '🍛';
    if (n.includes('chicken'))  return '🍗';
    if (n.includes('drink'))    return '🥤';
    if (n.includes('coffee'))   return '☕';
    if (n.includes('dessert'))  return '🍰';
    if (n.includes('sandwich')) return '🥪';
    if (n.includes('chinese'))  return '🍜';
    return '🍽️';
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const category = [...this.selectedCats()].join(',');
    this.dialogRef.close({ ...this.form.value, category });
  }

  cancel(): void {
    this.dialogRef.close();
  }
}

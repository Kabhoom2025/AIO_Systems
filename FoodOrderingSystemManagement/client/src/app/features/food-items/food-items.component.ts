import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDialog } from '@angular/material/dialog';
import { forkJoin } from 'rxjs';
import { FoodItemService } from '../../core/services/food-item.service';
import { CategoryService } from '../../core/services/category.service';
import { NotificationService } from '../../shared/services/notification.service';
import { FoodItem } from '../../core/models/food-item.model';
import { Category } from '../../core/models/category.model';
import { FoodItemFormDialogComponent } from './food-item-form-dialog/food-item-form-dialog.component';
import { ConfirmationDialogComponent } from '../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { BarcodePrintDialogComponent } from '../../shared/components/barcode-print-dialog/barcode-print-dialog.component';

@Component({
  selector: 'app-food-items',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatChipsModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatBadgeModule,
  ],
  templateUrl: './food-items.component.html',
  styleUrl: './food-items.component.scss',
})
export class FoodItemsComponent implements OnInit {
  private foodItemService = inject(FoodItemService);
  private categoryService = inject(CategoryService);
  private dialog          = inject(MatDialog);
  private notify          = inject(NotificationService);

  allItems    = signal<FoodItem[]>([]);
  categories  = signal<Category[]>([]);
  loading     = signal(false);
  searchTerm  = signal('');
  selectedCategoryId = signal<number | null>(null);

  filtered = computed(() => {
    let items = this.allItems();
    const cat = this.selectedCategoryId();
    const term = this.searchTerm().toLowerCase();
    if (cat !== null) items = items.filter((i) => i.categoryId === cat);
    if (term)         items = items.filter((i) =>
      i.itemName.toLowerCase().includes(term) ||
      (i.description ?? '').toLowerCase().includes(term)
    );
    return items;
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    forkJoin({
      items:      this.foodItemService.getAll(),
      categories: this.categoryService.getAll(),
    }).subscribe({
      next: ({ items, categories }) => {
        this.allItems.set(items.data ?? []);
        this.categories.set((categories.data ?? []).filter((c) => c.isActive));
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load food items.');
        this.loading.set(false);
      },
    });
  }

  selectCategory(id: number | null): void {
    this.selectedCategoryId.set(id);
  }

  onSearch(value: string): void {
    this.searchTerm.set(value);
  }

  countByCategory(id: number): number {
    return this.allItems().filter((i) => i.categoryId === id).length;
  }

  openCreate(): void {
    const ref = this.dialog.open(FoodItemFormDialogComponent, {
      data: { categories: this.categories() },
      width: '560px',
      disableClose: true,
    });
    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.foodItemService.create(result).subscribe({
        next: (res) => {
          this.notify.success('Food item created successfully.');
          this.allItems.update((list) => [...list, res.data]);
        },
        error: (err) => this.notify.error(err?.error?.message || 'Create failed.'),
      });
    });
  }

  openBarcodePrint(item: FoodItem): void {
    if (!item.barcode) {
      this.notify.error('This item has no barcode assigned. Edit the item to add one.');
      return;
    }
    this.dialog.open(BarcodePrintDialogComponent, {
      data: { title: item.itemName, barcode: item.barcode },
      width: '400px',
    });
  }

  openEdit(item: FoodItem): void {
    const ref = this.dialog.open(FoodItemFormDialogComponent, {
      data: { foodItem: item, categories: this.categories() },
      width: '560px',
      disableClose: true,
    });
    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.foodItemService.update(item.id, result).subscribe({
        next: (res) => {
          this.notify.success('Food item updated successfully.');
          this.allItems.update((list) =>
            list.map((i) => (i.id === item.id ? res.data : i))
          );
        },
        error: (err) => this.notify.error(err?.error?.message || 'Update failed.'),
      });
    });
  }

  confirmDelete(item: FoodItem): void {
    const ref = this.dialog.open(ConfirmationDialogComponent, {
      data: {
        title: 'Delete Food Item',
        message: `Delete "${item.itemName}"? This cannot be undone.`,
        confirmText: 'Delete',
        danger: true,
      },
      width: '420px',
    });
    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.foodItemService.delete(item.id).subscribe({
        next: () => {
          this.notify.success('Food item deleted.');
          this.allItems.update((list) => list.filter((i) => i.id !== item.id));
        },
        error: (err) => this.notify.error(err?.error?.message || 'Delete failed.'),
      });
    });
  }

  getStockClass(qty: number): string {
    if (qty === 0)  return 'out';
    if (qty <= 5)   return 'low';
    return 'ok';
  }
}

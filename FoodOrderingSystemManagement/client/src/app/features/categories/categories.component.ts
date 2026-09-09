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
import { MatDialog } from '@angular/material/dialog';
import { MatMenuModule } from '@angular/material/menu';
import { CategoryService } from '../../core/services/category.service';
import { NotificationService } from '../../shared/services/notification.service';
import { Category } from '../../core/models/category.model';
import { CategoryFormDialogComponent } from './category-form-dialog/category-form-dialog.component';
import { ConfirmationDialogComponent } from '../../shared/components/confirmation-dialog/confirmation-dialog.component';

@Component({
  selector: 'app-categories',
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
    MatMenuModule,
  ],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.scss',
})
export class CategoriesComponent implements OnInit {
  private categoryService = inject(CategoryService);
  private dialog = inject(MatDialog);
  private notify = inject(NotificationService);

  categories = signal<Category[]>([]);
  loading = signal(false);
  searchTerm = signal('');

  filtered = computed(() => {
    const term = this.searchTerm().toLowerCase();
    return this.categories().filter((c) =>
      c.categoryName.toLowerCase().includes(term)
    );
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.categoryService.getAll().subscribe({
      next: (res) => {
        this.categories.set(res.data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load categories.');
        this.loading.set(false);
      },
    });
  }

  openCreate(): void {
    const ref = this.dialog.open(CategoryFormDialogComponent, {
      data: {},
      width: '500px',
      disableClose: true,
    });
    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.categoryService.create(result).subscribe({
        next: (res) => {
          this.notify.success('Category created successfully.');
          this.categories.update((list) => [...list, res.data]);
        },
        error: (err) => this.notify.error(err?.error?.message || 'Create failed.'),
      });
    });
  }

  openEdit(category: Category): void {
    const ref = this.dialog.open(CategoryFormDialogComponent, {
      data: { category },
      width: '500px',
      disableClose: true,
    });
    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.categoryService.update(category.id, result).subscribe({
        next: (res) => {
          this.notify.success('Category updated successfully.');
          this.categories.update((list) =>
            list.map((c) => (c.id === category.id ? res.data : c))
          );
        },
        error: (err) => this.notify.error(err?.error?.message || 'Update failed.'),
      });
    });
  }

  confirmDelete(category: Category): void {
    const ref = this.dialog.open(ConfirmationDialogComponent, {
      data: {
        title: 'Delete Category',
        message: `Are you sure you want to delete "${category.categoryName}"? This action cannot be undone.`,
        confirmText: 'Delete',
        danger: true,
      },
      width: '420px',
    });
    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.categoryService.delete(category.id).subscribe({
        next: () => {
          this.notify.success('Category deleted.');
          this.categories.update((list) => list.filter((c) => c.id !== category.id));
        },
        error: (err) => this.notify.error(err?.error?.message || 'Delete failed.'),
      });
    });
  }

  onSearch(value: string): void {
    this.searchTerm.set(value);
  }

  getCategoryIcon(name: string): string {
    const n = name.toLowerCase();
    if (n.includes('pizza'))    return '🍕';
    if (n.includes('burger'))   return '🍔';
    if (n.includes('biryani'))  return '🍛';
    if (n.includes('drink'))    return '🥤';
    if (n.includes('ice'))      return '🍦';
    if (n.includes('dessert'))  return '🍰';
    if (n.includes('chinese'))  return '🍜';
    if (n.includes('sandwich')) return '🥪';
    if (n.includes('chicken'))  return '🍗';
    if (n.includes('soup'))     return '🍲';
    if (n.includes('salad'))    return '🥗';
    if (n.includes('coffee'))   return '☕';
    return '🍽️';
  }
}

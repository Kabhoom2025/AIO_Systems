import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDialog } from '@angular/material/dialog';
import { AddOnService } from '../../core/services/addon.service';
import { NotificationService } from '../../shared/services/notification.service';
import { ConfirmationDialogComponent } from '../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { AddOnFormDialogComponent } from './addon-form-dialog.component';
import { AddOn } from '../../core/models/addon.model';

@Component({
  selector: 'app-addons',
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
    MatSlideToggleModule,
  ],
  templateUrl: './addons.component.html',
  styleUrl: './addons.component.scss',
})
export class AddOnsComponent implements OnInit {
  private addOnService = inject(AddOnService);
  private dialog       = inject(MatDialog);
  private notify       = inject(NotificationService);

  addOns     = signal<AddOn[]>([]);
  loading    = signal(false);
  searchTerm = signal('');

  filtered = computed(() => {
    const term = this.searchTerm().toLowerCase();
    return this.addOns().filter(a =>
      a.name.toLowerCase().includes(term) ||
      (a.category ?? '').toLowerCase().includes(term)
    );
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.addOnService.getAll().subscribe({
      next: (res) => {
        this.addOns.set(res.data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load add-ons.');
        this.loading.set(false);
      },
    });
  }

  openCreate(): void {
    const ref = this.dialog.open(AddOnFormDialogComponent, {
      data: {},
      width: '460px',
      disableClose: true,
    });
    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.addOnService.create(result).subscribe({
        next: (res) => {
          this.notify.success('Add-on created.');
          this.addOns.update(list => [...list, res.data]);
        },
        error: (err) => this.notify.error(err?.error?.message || 'Create failed.'),
      });
    });
  }

  openEdit(addOn: AddOn): void {
    const ref = this.dialog.open(AddOnFormDialogComponent, {
      data: { addOn },
      width: '460px',
      disableClose: true,
    });
    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.addOnService.update(addOn.id, result).subscribe({
        next: (res) => {
          this.notify.success('Add-on updated.');
          this.addOns.update(list => list.map(a => a.id === addOn.id ? res.data : a));
        },
        error: (err) => this.notify.error(err?.error?.message || 'Update failed.'),
      });
    });
  }

  confirmDelete(addOn: AddOn): void {
    const ref = this.dialog.open(ConfirmationDialogComponent, {
      data: {
        title: 'Delete Add-On',
        message: `Delete "${addOn.name}"? This cannot be undone.`,
        confirmText: 'Delete',
        danger: true,
      },
      width: '400px',
    });
    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.addOnService.delete(addOn.id).subscribe({
        next: () => {
          this.notify.success('Add-on deleted.');
          this.addOns.update(list => list.filter(a => a.id !== addOn.id));
        },
        error: (err) => this.notify.error(err?.error?.message || 'Delete failed.'),
      });
    });
  }

  getCategoryEmoji(category?: string): string {
    const c = (category ?? '').toLowerCase();
    if (c.includes('pizza'))    return '🍕';
    if (c.includes('burger'))   return '🍔';
    if (c.includes('biryani'))  return '🍛';
    if (c.includes('chicken'))  return '🍗';
    if (c.includes('drink'))    return '🥤';
    if (c.includes('coffee'))   return '☕';
    if (c.includes('dessert'))  return '🍰';
    if (c.includes('sandwich')) return '🥪';
    if (c.includes('chinese'))  return '🍜';
    return '🍽️';
  }
}

import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { RouterModule } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { DeliveryService } from '../services/delivery.service';
import { DeliveryChargeSlabDto } from '../models/delivery.model';

@Component({
  selector: 'app-delivery-charges',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatProgressSpinnerModule, MatSnackBarModule, RouterModule],
  templateUrl: './delivery-charges.component.html',
  styleUrl: './delivery-charges.component.scss',
})
export class DeliveryChargesComponent implements OnInit, OnDestroy {
  private svc     = inject(DeliveryService);
  private snack   = inject(MatSnackBar);
  private destroy = new Subject<void>();

  loading = signal(true);
  slabs   = signal<DeliveryChargeSlabDto[]>([]);
  saving  = signal(false);

  newSlab = { fromKm: 0, toKm: 0, charge: 0 };

  ngOnInit(): void { this.load(); }
  ngOnDestroy(): void { this.destroy.next(); this.destroy.complete(); }

  load(): void {
    this.loading.set(true);
    this.svc.getChargeSlabs().pipe(takeUntil(this.destroy)).subscribe({
      next: list => { this.slabs.set(list); this.loading.set(false); },
      error: () => {
        this.loading.set(false);
        this.snack.open('Failed to load charge slabs.', 'Retry', { duration: 5000, panelClass: 'snack-error' })
          .onAction().subscribe(() => this.load());
      },
    });
  }

  addSlab(): void {
    if (this.newSlab.toKm <= this.newSlab.fromKm) return;
    this.saving.set(true);
    this.svc.addChargeSlab(this.newSlab).pipe(takeUntil(this.destroy)).subscribe({
      next: () => {
        this.load();
        this.newSlab = { fromKm: 0, toKm: 0, charge: 0 };
        this.saving.set(false);
        this.snack.open('Charge slab added.', 'Close', { duration: 2500, panelClass: 'snack-success' });
      },
      error: (err) => {
        this.saving.set(false);
        const msg = err?.error?.message ?? 'Failed to add charge slab.';
        this.snack.open(msg, 'Close', { duration: 4000, panelClass: 'snack-error' });
      },
    });
  }

  removeSlab(id: number): void {
    if (!confirm('Delete this charge slab?')) return;
    this.svc.deleteChargeSlab(id).pipe(takeUntil(this.destroy)).subscribe({
      next: () => {
        this.load();
        this.snack.open('Charge slab deleted.', 'Close', { duration: 2500 });
      },
      error: () => this.snack.open('Failed to delete charge slab.', 'Close', { duration: 3000, panelClass: 'snack-error' }),
    });
  }
}

import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { RouterModule } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { DeliveryService } from '../services/delivery.service';
import { DriverDto, CreateDriverDto, UpdateDriverDto } from '../models/delivery.model';

@Component({
  selector: 'app-delivery-drivers',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule, MatSnackBarModule, RouterModule],
  templateUrl: './delivery-drivers.component.html',
  styleUrl: './delivery-drivers.component.scss',
})
export class DeliveryDriversComponent implements OnInit, OnDestroy {
  private svc     = inject(DeliveryService);
  private snack   = inject(MatSnackBar);
  private destroy = new Subject<void>();

  loading = signal(true);
  drivers = signal<DriverDto[]>([]);
  showForm = signal(false);
  editingDriver = signal<DriverDto | null>(null);
  saving = signal(false);
  loadError = signal(false);

  form: CreateDriverDto = this.emptyForm();

  ngOnInit(): void { this.load(); }
  ngOnDestroy(): void { this.destroy.next(); this.destroy.complete(); }

  load(): void {
    this.loading.set(true);
    this.loadError.set(false);
    this.svc.getDrivers().pipe(takeUntil(this.destroy)).subscribe({
      next: list => { this.drivers.set(list); this.loading.set(false); },
      error: () => {
        this.loading.set(false);
        this.loadError.set(true);
        this.snack.open('Failed to load drivers.', 'Retry', { duration: 5000, panelClass: 'snack-error' })
          .onAction().subscribe(() => this.load());
      },
    });
  }

  openCreate(): void {
    this.editingDriver.set(null);
    this.form = this.emptyForm();
    this.showForm.set(true);
  }

  openEdit(d: DriverDto): void {
    this.editingDriver.set(d);
    this.form = { name: d.name, phone: d.phone, email: d.email, vehicleNo: d.vehicleNo, vehicleType: d.vehicleType };
    this.showForm.set(true);
  }

  save(): void {
    this.saving.set(true);
    const editing = this.editingDriver();
    const isEdit = !!editing;
    const obs = editing
      ? this.svc.updateDriver(editing.id, { ...this.form, isAvailable: editing.isAvailable, isActive: editing.isActive })
      : this.svc.createDriver(this.form);

    obs.pipe(takeUntil(this.destroy)).subscribe({
      next: () => {
        this.load();
        this.showForm.set(false);
        this.saving.set(false);
        this.snack.open(isEdit ? 'Driver updated.' : 'Driver added.', 'Close', { duration: 3000, panelClass: 'snack-success' });
      },
      error: (err) => {
        this.saving.set(false);
        const msg = err?.error?.message ?? 'Failed to save driver.';
        this.snack.open(msg, 'Close', { duration: 4000, panelClass: 'snack-error' });
      },
    });
  }

  toggleAvailability(d: DriverDto): void {
    const dto: UpdateDriverDto = {
      name: d.name, phone: d.phone, email: d.email, vehicleNo: d.vehicleNo,
      vehicleType: d.vehicleType, isAvailable: !d.isAvailable, isActive: d.isActive,
    };
    this.svc.updateDriver(d.id, dto).pipe(takeUntil(this.destroy)).subscribe({
      next: () => {
        this.load();
        this.snack.open(`${d.name} marked as ${!d.isAvailable ? 'Available' : 'Busy'}.`, 'Close', { duration: 2500 });
      },
      error: () => this.snack.open('Failed to update driver status.', 'Close', { duration: 3000, panelClass: 'snack-error' }),
    });
  }

  deleteDriver(d: DriverDto): void {
    if (!confirm(`Deactivate driver "${d.name}"?`)) return;
    this.svc.deleteDriver(d.id).pipe(takeUntil(this.destroy)).subscribe({
      next: () => {
        this.load();
        this.snack.open(`${d.name} deactivated.`, 'Close', { duration: 2500 });
      },
      error: () => this.snack.open('Failed to deactivate driver.', 'Close', { duration: 3000, panelClass: 'snack-error' }),
    });
  }

  private emptyForm(): CreateDriverDto {
    return { name: '', phone: '', email: '', vehicleNo: '', vehicleType: '' };
  }
}

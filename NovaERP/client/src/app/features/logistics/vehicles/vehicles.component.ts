import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import { VehicleApiService, VehicleDto, CreateVehicleDto, UpdateVehicleDto } from '../../../core/vehicle-api.service';
import { WarehouseApiService, WarehouseDto } from '../../../core/warehouse-api.service';

@Component({
  selector: 'app-vehicles',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, DropdownModule, CheckboxModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './vehicles.component.html',
  styleUrl: './vehicles.component.scss'
})
export class VehiclesComponent implements OnInit {
  vehicles: VehicleDto[] = [];
  warehouses: WarehouseDto[] = [];
  loading = false;

  showDialog = false;
  editing: VehicleDto | null = null;
  saving = false;

  form: { code: string; name: string; model: string | null; capacityKg: number | null; currentWarehouseId: number | null; isActive: boolean } = this.emptyForm();

  constructor(
    private api: VehicleApiService,
    private warehouseApi: WarehouseApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.warehouseApi.getAll().subscribe({ next: rows => (this.warehouses = rows), error: () => (this.warehouses = []) });
  }

  private emptyForm() {
    return { code: '', name: '', model: null, capacityKg: null, currentWarehouseId: null, isActive: true };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.vehicles = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load vehicles.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(vehicle: VehicleDto) {
    this.editing = vehicle;
    this.form = {
      code: vehicle.code, name: vehicle.name, model: vehicle.model,
      capacityKg: vehicle.capacityKg, currentWarehouseId: vehicle.currentWarehouseId, isActive: vehicle.isActive
    };
    this.showDialog = true;
  }

  save() {
    if (!this.form.name || (!this.editing && !this.form.code) || !this.form.capacityKg) {
      this.notify.warn('Code, name, and capacity are required.');
      return;
    }
    this.saving = true;
    const payload = {
      name: this.form.name,
      model: this.form.model,
      capacityKg: this.form.capacityKg,
      currentWarehouseId: this.form.currentWarehouseId,
      isActive: this.form.isActive
    };
    const req$ = this.editing
      ? this.api.update(this.editing.id, payload as UpdateVehicleDto)
      : this.api.create({ ...payload, code: this.form.code } as CreateVehicleDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Vehicle ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save vehicle.');
      }
    });
  }

  delete(vehicle: VehicleDto) {
    this.confirm.confirm({
      message: `Delete vehicle "${vehicle.code}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(vehicle.id).subscribe({
          next: () => {
            this.notify.success('Vehicle deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete vehicle.')
        });
      }
    });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Available') return 'success';
    if (status === 'InTransit') return 'warn';
    if (status === 'Maintenance') return 'danger';
    return 'info';
  }
}

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  AssetApiService, AssetDto, CreateAssetDto, UpdateAssetDto
} from '../../../core/asset-api.service';
import { AssetCategoryApiService, AssetCategoryDto } from '../../../core/asset-category-api.service';
import { EmployeeApiService, EmployeeDto } from '../../../core/employee-api.service';

@Component({
  selector: 'app-assets',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, DropdownModule, CalendarModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './assets.component.html',
  styleUrl: './assets.component.scss'
})
export class AssetsComponent implements OnInit {
  assets: AssetDto[] = [];
  categories: AssetCategoryDto[] = [];
  employees: EmployeeDto[] = [];
  loading = false;

  showDialog = false;
  editing: AssetDto | null = null;
  saving = false;

  form: {
    name: string;
    categoryId: number | null;
    serialNumber: string | null;
    purchaseDate: Date | null;
    purchaseCost: number | null;
    warrantyExpiryDate: Date | null;
  } = this.emptyForm();

  showAssignDialog = false;
  assigning: AssetDto | null = null;
  assignEmployeeId: number | null = null;

  constructor(
    private api: AssetApiService,
    private categoryApi: AssetCategoryApiService,
    private employeeApi: EmployeeApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.categoryApi.getAll().subscribe({ next: rows => (this.categories = rows), error: () => (this.categories = []) });
    this.employeeApi.getAll().subscribe({ next: rows => (this.employees = rows), error: () => (this.employees = []) });
  }

  private emptyForm() {
    return {
      name: '', categoryId: null, serialNumber: null,
      purchaseDate: new Date(), purchaseCost: null, warrantyExpiryDate: null
    };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.assets = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load assets.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(asset: AssetDto) {
    this.editing = asset;
    this.form = {
      name: asset.name, categoryId: asset.categoryId, serialNumber: asset.serialNumber,
      purchaseDate: new Date(asset.purchaseDate),
      purchaseCost: asset.purchaseCost,
      warrantyExpiryDate: asset.warrantyExpiryDate ? new Date(asset.warrantyExpiryDate) : null
    };
    this.showDialog = true;
  }

  save() {
    if (!this.form.name || !this.form.categoryId) {
      this.notify.warn('Name and category are required.');
      return;
    }
    this.saving = true;
    const payload = {
      name: this.form.name,
      categoryId: this.form.categoryId,
      serialNumber: this.form.serialNumber,
      purchaseDate: (this.form.purchaseDate ?? new Date()).toISOString(),
      purchaseCost: this.form.purchaseCost,
      warrantyExpiryDate: this.form.warrantyExpiryDate ? this.form.warrantyExpiryDate.toISOString() : null
    };
    const req$ = this.editing
      ? this.api.update(this.editing.id, payload as UpdateAssetDto)
      : this.api.create(payload as CreateAssetDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Asset ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save asset.');
      }
    });
  }

  delete(asset: AssetDto) {
    this.confirm.confirm({
      message: `Delete asset "${asset.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(asset.id).subscribe({
          next: () => {
            this.notify.success('Asset deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete asset.')
        });
      }
    });
  }

  openAssign(asset: AssetDto) {
    this.assigning = asset;
    this.assignEmployeeId = null;
    this.showAssignDialog = true;
  }

  confirmAssign() {
    if (!this.assigning || !this.assignEmployeeId) {
      this.notify.warn('Select an employee.');
      return;
    }
    this.api.assign(this.assigning.id, { employeeId: this.assignEmployeeId }).subscribe({
      next: () => {
        this.showAssignDialog = false;
        this.notify.success('Asset assigned.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to assign asset.')
    });
  }

  unassign(asset: AssetDto) {
    this.api.unassign(asset.id).subscribe({
      next: () => {
        this.notify.success('Asset unassigned.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to unassign asset.')
    });
  }

  retire(asset: AssetDto) {
    this.confirm.confirm({
      message: `Retire asset "${asset.name}"? This cannot be undone.`,
      header: 'Confirm Retire',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.retire(asset.id).subscribe({
          next: () => {
            this.notify.success('Asset retired.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to retire asset.')
        });
      }
    });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Available') return 'success';
    if (status === 'Retired') return 'danger';
    if (status === 'UnderMaintenance') return 'warn';
    return 'info';
  }
}

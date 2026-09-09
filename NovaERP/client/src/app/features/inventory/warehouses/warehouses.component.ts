import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  WarehouseApiService, WarehouseDto, CreateWarehouseDto, UpdateWarehouseDto
} from '../../../core/warehouse-api.service';
import { BranchApiService, BranchDto } from '../../../core/branch-api.service';

@Component({
  selector: 'app-warehouses',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, DropdownModule, CheckboxModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './warehouses.component.html',
  styleUrl: './warehouses.component.scss'
})
export class WarehousesComponent implements OnInit {
  warehouses: WarehouseDto[] = [];
  branches: BranchDto[] = [];
  loading = false;

  showDialog = false;
  editing: WarehouseDto | null = null;
  saving = false;

  form: {
    branchId: number | null; name: string; code: string;
    contactName: string | null; email: string | null; phone: string | null;
    address: string | null; addressLine2: string | null;
    city: string | null; state: string | null; postalCode: string | null; country: string | null;
    taxType: string | null; taxCountry: string | null; taxId: string | null;
    isActive: boolean;
  } = this.emptyForm();

  constructor(
    private api: WarehouseApiService,
    private branchApi: BranchApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.branchApi.getAll().subscribe({ next: rows => (this.branches = rows), error: () => (this.branches = []) });
  }

  private emptyForm() {
    return {
      branchId: null, name: '', code: '',
      contactName: null, email: null, phone: null,
      address: null, addressLine2: null,
      city: null, state: null, postalCode: null, country: null,
      taxType: null, taxCountry: null, taxId: null,
      isActive: true
    };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.warehouses = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load warehouses.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(warehouse: WarehouseDto) {
    this.editing = warehouse;
    this.form = {
      branchId: warehouse.branchId, name: warehouse.name, code: warehouse.code,
      contactName: warehouse.contactName, email: warehouse.email, phone: warehouse.phone,
      address: warehouse.address, addressLine2: warehouse.addressLine2,
      city: warehouse.city, state: warehouse.state,
      postalCode: warehouse.postalCode, country: warehouse.country,
      taxType: warehouse.taxType, taxCountry: warehouse.taxCountry, taxId: warehouse.taxId,
      isActive: warehouse.isActive
    };
    this.showDialog = true;
  }

  save() {
    if (!this.form.branchId || !this.form.name || (!this.editing && !this.form.code)) {
      this.notify.warn('Branch, name, and code are required.');
      return;
    }
    this.saving = true;
    const payload = {
      branchId: this.form.branchId,
      name: this.form.name,
      contactName: this.form.contactName,
      email: this.form.email,
      phone: this.form.phone,
      address: this.form.address,
      addressLine2: this.form.addressLine2,
      city: this.form.city,
      state: this.form.state,
      postalCode: this.form.postalCode,
      country: this.form.country,
      taxType: this.form.taxType,
      taxCountry: this.form.taxCountry,
      taxId: this.form.taxId,
      isActive: this.form.isActive
    };
    const req$ = this.editing
      ? this.api.update(this.editing.id, payload as UpdateWarehouseDto)
      : this.api.create({ ...payload, code: this.form.code } as CreateWarehouseDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Warehouse ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save warehouse.');
      }
    });
  }

  delete(warehouse: WarehouseDto) {
    this.confirm.confirm({
      message: `Delete warehouse "${warehouse.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(warehouse.id).subscribe({
          next: () => {
            this.notify.success('Warehouse deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete warehouse.')
        });
      }
    });
  }
}

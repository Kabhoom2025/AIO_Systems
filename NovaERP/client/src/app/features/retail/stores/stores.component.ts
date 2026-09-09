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
  StoreApiService, StoreDto, CreateStoreDto, UpdateStoreDto
} from '../../../core/store-api.service';
import { BranchApiService, BranchDto } from '../../../core/branch-api.service';
import { WarehouseApiService, WarehouseDto } from '../../../core/warehouse-api.service';

@Component({
  selector: 'app-stores',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, DropdownModule, CheckboxModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './stores.component.html',
  styleUrl: './stores.component.scss'
})
export class StoresComponent implements OnInit {
  stores: StoreDto[] = [];
  branches: BranchDto[] = [];
  warehouses: WarehouseDto[] = [];
  loading = false;

  showDialog = false;
  editing: StoreDto | null = null;
  saving = false;

  form: { branchId: number | null; warehouseId: number | null; name: string; code: string; address: string | null; isActive: boolean } = this.emptyForm();

  constructor(
    private api: StoreApiService,
    private branchApi: BranchApiService,
    private warehouseApi: WarehouseApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.branchApi.getAll().subscribe({ next: rows => (this.branches = rows), error: () => (this.branches = []) });
    this.warehouseApi.getAll().subscribe({ next: rows => (this.warehouses = rows), error: () => (this.warehouses = []) });
  }

  private emptyForm() {
    return { branchId: null, warehouseId: null, name: '', code: '', address: null, isActive: true };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.stores = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load stores.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(store: StoreDto) {
    this.editing = store;
    this.form = {
      branchId: store.branchId, warehouseId: store.warehouseId, name: store.name, code: store.code,
      address: store.address, isActive: store.isActive
    };
    this.showDialog = true;
  }

  save() {
    if (!this.form.branchId || !this.form.warehouseId || !this.form.name || (!this.editing && !this.form.code)) {
      this.notify.warn('Branch, warehouse, name, and code are required.');
      return;
    }
    this.saving = true;
    const payload = {
      branchId: this.form.branchId,
      warehouseId: this.form.warehouseId,
      name: this.form.name,
      address: this.form.address,
      isActive: this.form.isActive
    };
    const req$ = this.editing
      ? this.api.update(this.editing.id, payload as UpdateStoreDto)
      : this.api.create({ ...payload, code: this.form.code } as CreateStoreDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Store ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save store.');
      }
    });
  }

  delete(store: StoreDto) {
    this.confirm.confirm({
      message: `Delete store "${store.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(store.id).subscribe({
          next: () => {
            this.notify.success('Store deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete store.')
        });
      }
    });
  }
}

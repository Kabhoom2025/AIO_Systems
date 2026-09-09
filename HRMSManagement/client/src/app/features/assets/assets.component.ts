import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextarea } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { CalendarModule } from 'primeng/calendar';
import { TabViewModule } from 'primeng/tabview';
import { TooltipModule } from 'primeng/tooltip';

import {
  AssetApiService,
  AssetDto,
  AssetDetailDto,
  CreateAssetDto,
  UpdateAssetDto
} from '../../core/asset-api.service';
import { environment } from '../../../environments/environment';
import { NotificationService } from '../../core/notification.service';
import { HasPermissionDirective } from '../../core/permission.directive';

interface EmployeeLookupDto {
  id: number;
  employeeCode: string;
  fullName: string;
  designationTitle: string;
}

const CATEGORIES = ['Laptop', 'Desktop', 'Mobile', 'Monitor', 'Accessory', 'Furniture', 'Other'];
const STATUSES = ['Available', 'Allocated', 'InRepair', 'Retired'];
const CONDITIONS = ['New', 'Good', 'Fair', 'Damaged'];

@Component({
  selector: 'app-assets',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    InputTextarea,
    DropdownModule,
    TagModule,
    CalendarModule,
    TabViewModule,
    TooltipModule,
    HasPermissionDirective
  ],
  templateUrl: './assets.component.html',
  styleUrl: './assets.component.scss'
})
export class AssetsComponent implements OnInit {
  assets: AssetDto[] = [];
  myAssets: AssetDto[] = [];
  employees: EmployeeLookupDto[] = [];
  loading = false;

  categories = CATEGORIES;
  statuses = STATUSES;
  conditions = CONDITIONS;

  categoryFilter: string | null = null;
  statusFilter: string | null = null;

  // Add/Edit dialog
  showFormDialog = false;
  editId: number | null = null;
  form: CreateAssetDto & { status: string } = this.emptyForm();

  // View dialog
  showViewDialog = false;
  viewAsset: AssetDetailDto | null = null;

  // Allocate dialog
  showAllocateDialog = false;
  allocateAsset: AssetDto | null = null;
  allocateForm = { employeeId: null as number | null, notes: '' };

  // Return dialog
  showReturnDialog = false;
  returnAsset: AssetDto | null = null;
  returnForm = { returnCondition: 'Good', notes: '' };

  constructor(
    private assetApi: AssetApiService,
    private http: HttpClient,
    private notify: NotificationService
  ) {}

  ngOnInit() {
    this.loadAssets();
    this.loadMyAssets();
    this.loadEmployees();
  }

  get availableCount() {
    return this.assets.filter(a => a.status === 'Available').length;
  }
  get allocatedCount() {
    return this.assets.filter(a => a.status === 'Allocated').length;
  }
  get inRepairCount() {
    return this.assets.filter(a => a.status === 'InRepair').length;
  }
  get retiredCount() {
    return this.assets.filter(a => a.status === 'Retired').length;
  }

  loadAssets() {
    this.loading = true;
    this.assetApi.getAll(this.categoryFilter, this.statusFilter).subscribe({
      next: data => {
        this.assets = data;
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load assets.');
      }
    });
  }

  loadMyAssets() {
    this.assetApi.getMy().subscribe({
      next: data => (this.myAssets = data),
      error: () => {}
    });
  }

  loadEmployees() {
    this.http.get<EmployeeLookupDto[]>(`${environment.apiUrl}/employees/lookup`).subscribe({
      next: data => (this.employees = data),
      error: () => {}
    });
  }

  onFilterChange() {
    this.loadAssets();
  }

  emptyForm(): CreateAssetDto & { status: string } {
    return {
      name: '',
      category: 'Laptop',
      serialNumber: '',
      purchaseDate: null,
      purchaseCost: null,
      warrantyUntil: null,
      condition: 'Good',
      branchId: null,
      notes: '',
      status: 'Available'
    };
  }

  openAdd() {
    this.editId = null;
    this.form = this.emptyForm();
    this.showFormDialog = true;
  }

  openEdit(asset: AssetDto) {
    this.editId = asset.id;
    this.form = {
      name: asset.name,
      category: asset.category,
      serialNumber: asset.serialNumber ?? '',
      purchaseDate: asset.purchaseDate,
      purchaseCost: asset.purchaseCost,
      warrantyUntil: asset.warrantyUntil,
      condition: asset.condition,
      branchId: asset.branchId,
      notes: asset.notes ?? '',
      status: asset.status
    };
    this.showFormDialog = true;
  }

  save() {
    if (!this.form.name.trim()) {
      this.notify.warn('Asset name is required.');
      return;
    }

    if (this.editId) {
      const dto: UpdateAssetDto = { ...this.form };
      this.assetApi.update(this.editId, dto).subscribe({
        next: () => {
          this.notify.success('Asset updated.');
          this.showFormDialog = false;
          this.loadAssets();
        },
        error: err => this.notify.error(err.error?.message ?? 'Failed to update asset.')
      });
    } else {
      const dto: CreateAssetDto = { ...this.form };
      this.assetApi.create(dto).subscribe({
        next: () => {
          this.notify.success('Asset created.');
          this.showFormDialog = false;
          this.loadAssets();
        },
        error: err => this.notify.error(err.error?.message ?? 'Failed to create asset.')
      });
    }
  }

  deleteAsset(asset: AssetDto) {
    if (!confirm(`Delete asset "${asset.name}"?`)) return;
    this.assetApi.delete(asset.id).subscribe({
      next: () => {
        this.notify.success('Asset deleted.');
        this.loadAssets();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to delete asset.')
    });
  }

  viewDetail(asset: AssetDto) {
    this.assetApi.getById(asset.id).subscribe({
      next: data => {
        this.viewAsset = data;
        this.showViewDialog = true;
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to load asset details.')
    });
  }

  openAllocate(asset: AssetDto) {
    this.allocateAsset = asset;
    this.allocateForm = { employeeId: null, notes: '' };
    this.showAllocateDialog = true;
  }

  submitAllocate() {
    if (!this.allocateAsset || !this.allocateForm.employeeId) {
      this.notify.warn('Please select an employee.');
      return;
    }
    this.assetApi
      .allocate(this.allocateAsset.id, {
        employeeId: this.allocateForm.employeeId,
        notes: this.allocateForm.notes
      })
      .subscribe({
        next: () => {
          this.notify.success('Asset allocated.');
          this.showAllocateDialog = false;
          this.loadAssets();
          this.loadMyAssets();
        },
        error: err => this.notify.error(err.error?.message ?? 'Failed to allocate asset.')
      });
  }

  openReturn(asset: AssetDto) {
    this.returnAsset = asset;
    this.returnForm = { returnCondition: asset.condition || 'Good', notes: '' };
    this.showReturnDialog = true;
  }

  submitReturn() {
    if (!this.returnAsset) return;
    this.assetApi.return(this.returnAsset.id, this.returnForm).subscribe({
      next: () => {
        this.notify.success('Asset returned.');
        this.showReturnDialog = false;
        this.loadAssets();
        this.loadMyAssets();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to return asset.')
    });
  }

  statusSeverity(status: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (status) {
      case 'Available':
        return 'success';
      case 'Allocated':
        return 'info';
      case 'InRepair':
        return 'warn';
      case 'Retired':
        return 'danger';
      default:
        return 'secondary';
    }
  }
}

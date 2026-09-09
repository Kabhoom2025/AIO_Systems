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
  VendorApiService, VendorDto, CreateVendorDto, UpdateVendorDto
} from '../../../core/vendor-api.service';
import { UserApiService, UserDto } from '../../../core/user-api.service';

@Component({
  selector: 'app-vendors',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, DropdownModule, CheckboxModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './vendors.component.html',
  styleUrl: './vendors.component.scss'
})
export class VendorsComponent implements OnInit {
  vendors: VendorDto[] = [];
  users: UserDto[] = [];
  loading = false;

  showDialog = false;
  editing: VendorDto | null = null;
  saving = false;

  form: { name: string; category: string | null; contactEmail: string | null; contactPhone: string | null; address: string | null; isActive: boolean; ownerId: number | null } = this.emptyForm();

  constructor(
    private api: VendorApiService,
    private userApi: UserApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.userApi.getAll().subscribe({ next: rows => (this.users = rows), error: () => (this.users = []) });
  }

  private emptyForm() {
    return { name: '', category: null, contactEmail: null, contactPhone: null, address: null, isActive: true, ownerId: null };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.vendors = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load vendors.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(vendor: VendorDto) {
    this.editing = vendor;
    this.form = {
      name: vendor.name, category: vendor.category, contactEmail: vendor.contactEmail,
      contactPhone: vendor.contactPhone, address: vendor.address, isActive: vendor.isActive, ownerId: vendor.ownerId
    };
    this.showDialog = true;
  }

  save() {
    if (!this.form.name || !this.form.ownerId) {
      this.notify.warn('Name and owner are required.');
      return;
    }
    this.saving = true;
    const payload = {
      name: this.form.name,
      category: this.form.category,
      contactEmail: this.form.contactEmail,
      contactPhone: this.form.contactPhone,
      address: this.form.address,
      isActive: this.form.isActive,
      ownerId: this.form.ownerId
    };
    const req$ = this.editing
      ? this.api.update(this.editing.id, payload as UpdateVendorDto)
      : this.api.create(payload as CreateVendorDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Vendor ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save vendor.');
      }
    });
  }

  delete(vendor: VendorDto) {
    this.confirm.confirm({
      message: `Delete vendor "${vendor.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(vendor.id).subscribe({
          next: () => {
            this.notify.success('Vendor deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete vendor.')
        });
      }
    });
  }
}

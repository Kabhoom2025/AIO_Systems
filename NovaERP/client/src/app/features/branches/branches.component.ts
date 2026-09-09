import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import { BranchApiService, BranchDto, CreateBranchDto, UpdateBranchDto } from '../../core/branch-api.service';

@Component({
  selector: 'app-branches',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, TagModule, ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './branches.component.html',
  styleUrl: './branches.component.scss'
})
export class BranchesComponent implements OnInit {
  branches: BranchDto[] = [];
  loading = false;

  showDialog = false;
  editing: BranchDto | null = null;
  form: CreateBranchDto & { isActive?: boolean } = this.emptyForm();
  saving = false;

  constructor(
    private api: BranchApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  private emptyForm(): CreateBranchDto & { isActive?: boolean } {
    return {
      name: '',
      code: '',
      address: null,
      city: null,
      state: null,
      country: null,
      phone: null,
      email: null,
      timezone: 'Asia/Kolkata',
      isHeadOffice: false
    };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.branches = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load branches.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(branch: BranchDto) {
    this.editing = branch;
    this.form = {
      name: branch.name,
      code: branch.code,
      address: branch.address,
      city: branch.city,
      state: branch.state,
      country: branch.country,
      phone: branch.phone,
      email: branch.email,
      timezone: branch.timezone,
      isHeadOffice: branch.isHeadOffice,
      isActive: branch.isActive
    };
    this.showDialog = true;
  }

  save() {
    if (!this.form.name.trim() || !this.form.code.trim()) {
      this.notify.warn('Name and code are required.');
      return;
    }
    this.saving = true;
    const req$ = this.editing
      ? this.api.update(this.editing.id, { ...this.form, isActive: this.form.isActive ?? true } as UpdateBranchDto)
      : this.api.create(this.form);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Branch ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save branch.');
      }
    });
  }

  delete(branch: BranchDto) {
    this.confirm.confirm({
      message: `Delete branch "${branch.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(branch.id).subscribe({
          next: () => {
            this.notify.success('Branch deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete branch.')
        });
      }
    });
  }
}

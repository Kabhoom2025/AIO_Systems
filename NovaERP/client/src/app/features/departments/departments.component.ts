import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import { DepartmentApiService, DepartmentDto, CreateDepartmentDto, UpdateDepartmentDto } from '../../core/department-api.service';
import { BranchApiService, BranchDto } from '../../core/branch-api.service';

@Component({
  selector: 'app-departments',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputTextarea, DropdownModule, TagModule, ToastModule,
    ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './departments.component.html',
  styleUrl: './departments.component.scss'
})
export class DepartmentsComponent implements OnInit {
  departments: DepartmentDto[] = [];
  branches: BranchDto[] = [];
  loading = false;

  showDialog = false;
  editing: DepartmentDto | null = null;
  form: CreateDepartmentDto & { isActive?: boolean } = this.emptyForm();
  saving = false;

  constructor(
    private api: DepartmentApiService,
    private branchApi: BranchApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.loadBranches();
  }

  private emptyForm(): CreateDepartmentDto & { isActive?: boolean } {
    return { branchId: null, parentId: null, name: '', code: '', description: null, costCenter: null };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.departments = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load departments.');
      }
    });
  }

  loadBranches() {
    this.branchApi.getAll().subscribe({
      next: rows => (this.branches = rows),
      error: () => (this.branches = [])
    });
  }

  get parentOptions(): DepartmentDto[] {
    return this.departments.filter(d => !this.editing || d.id !== this.editing.id);
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(dept: DepartmentDto) {
    this.editing = dept;
    this.form = {
      branchId: dept.branchId,
      parentId: dept.parentId,
      name: dept.name,
      code: dept.code,
      description: dept.description,
      costCenter: dept.costCenter,
      isActive: dept.isActive
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
      ? this.api.update(this.editing.id, { ...this.form, isActive: this.form.isActive ?? true } as UpdateDepartmentDto)
      : this.api.create(this.form);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Department ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save department.');
      }
    });
  }

  delete(dept: DepartmentDto) {
    this.confirm.confirm({
      message: `Delete department "${dept.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(dept.id).subscribe({
          next: () => {
            this.notify.success('Department deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete department.')
        });
      }
    });
  }
}

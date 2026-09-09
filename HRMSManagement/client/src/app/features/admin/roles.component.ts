import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { MultiSelectModule } from 'primeng/multiselect';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import { AdminApiService, RoleDto, CreateRoleDto, UpdateRoleDto, PermissionDto } from '../../core/admin-api.service';

interface PermissionOption {
  key: string;
  label: string;
}

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    TagModule,
    MultiSelectModule,
    ConfirmDialogModule,
    ToastModule,
    HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './roles.component.html',
  styleUrl: './roles.component.scss'
})
export class RolesComponent implements OnInit {
  roles: RoleDto[] = [];
  permissionOptions: PermissionOption[] = [];
  loading = false;

  showAddDialog = false;
  showEditDialog = false;
  saving = false;

  addForm: CreateRoleDto = this.emptyAddForm();
  editForm: UpdateRoleDto = this.emptyEditForm();
  editRoleId: number | null = null;

  constructor(
    private adminApi: AdminApiService,
    private notify: NotificationService,
    private confirmation: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.loadRoles();
    this.loadPermissions();
  }

  loadRoles() {
    this.loading = true;
    this.adminApi.getRoles().subscribe({
      next: data => {
        this.roles = data;
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load roles.');
      }
    });
  }

  loadPermissions() {
    this.adminApi.getPermissions().subscribe({
      next: (data: PermissionDto[]) => {
        this.permissionOptions = data.map(p => ({ key: p.key, label: `${p.module}: ${p.description}` }));
      },
      error: () => (this.permissionOptions = [])
    });
  }

  canDelete(role: RoleDto): boolean {
    return !role.isSystemRole && role.userCount === 0;
  }

  deleteDisabledReason(role: RoleDto): string {
    if (role.isSystemRole) return 'System roles cannot be deleted.';
    if (role.userCount > 0) return 'Role is assigned to users and cannot be deleted.';
    return '';
  }

  openAddDialog() {
    this.addForm = this.emptyAddForm();
    this.showAddDialog = true;
  }

  saveAdd() {
    this.saving = true;
    this.adminApi.createRole(this.addForm).subscribe({
      next: () => {
        this.saving = false;
        this.showAddDialog = false;
        this.notify.success('Role created successfully.');
        this.loadRoles();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to create role.');
      }
    });
  }

  openEditDialog(role: RoleDto) {
    if (role.isSystemRole) return;
    this.editRoleId = role.id;
    this.editForm = {
      name: role.name,
      description: role.description ?? '',
      permissionKeys: [...role.permissionKeys]
    };
    this.showEditDialog = true;
  }

  saveEdit() {
    if (this.editRoleId == null) return;
    this.saving = true;
    this.adminApi.updateRole(this.editRoleId, this.editForm).subscribe({
      next: () => {
        this.saving = false;
        this.showEditDialog = false;
        this.notify.success('Role updated successfully.');
        this.loadRoles();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to update role.');
      }
    });
  }

  confirmDelete(role: RoleDto) {
    if (!this.canDelete(role)) return;
    this.confirmation.confirm({
      message: `Delete role "${role.name}"? This cannot be undone.`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.delete(role)
    });
  }

  delete(role: RoleDto) {
    this.adminApi.deleteRole(role.id).subscribe({
      next: () => {
        this.notify.success('Role deleted.');
        this.loadRoles();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to delete role.')
    });
  }

  private emptyAddForm(): CreateRoleDto {
    return { name: '', description: '', permissionKeys: [] };
  }

  private emptyEditForm(): UpdateRoleDto {
    return { name: '', description: '', permissionKeys: [] };
  }
}

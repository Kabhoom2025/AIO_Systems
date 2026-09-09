import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import {
  AdminApiService,
  UserDto,
  CreateUserDto,
  UpdateUserDto,
  RoleDto,
  BranchLookupDto
} from '../../core/admin-api.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    DropdownModule,
    TagModule,
    ConfirmDialogModule,
    ToastModule,
    HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent implements OnInit {
  users: UserDto[] = [];
  roles: RoleDto[] = [];
  branches: BranchLookupDto[] = [];
  loading = false;

  showAddDialog = false;
  showEditDialog = false;
  showResetPasswordDialog = false;
  saving = false;

  addForm: CreateUserDto = this.emptyAddForm();
  editForm: UpdateUserDto = this.emptyEditForm();
  editUserId: number | null = null;

  resetPasswordForm = { newPassword: '' };
  resetPasswordUserId: number | null = null;

  constructor(
    private adminApi: AdminApiService,
    private notify: NotificationService,
    private confirmation: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.loadUsers();
    this.loadRoles();
    this.loadBranches();
  }

  loadUsers() {
    this.loading = true;
    this.adminApi.getUsers().subscribe({
      next: data => {
        this.users = data;
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load users.');
      }
    });
  }

  loadRoles() {
    this.adminApi.getRoles().subscribe({
      next: data => (this.roles = data),
      error: () => (this.roles = [])
    });
  }

  loadBranches() {
    this.adminApi.getBranches().subscribe({
      next: data => (this.branches = data),
      error: () => (this.branches = [])
    });
  }

  openAddDialog() {
    this.addForm = this.emptyAddForm();
    this.showAddDialog = true;
  }

  saveAdd() {
    this.saving = true;
    this.adminApi.createUser(this.addForm).subscribe({
      next: () => {
        this.saving = false;
        this.showAddDialog = false;
        this.notify.success('User created successfully.');
        this.loadUsers();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to create user.');
      }
    });
  }

  openEditDialog(user: UserDto) {
    this.editUserId = user.id;
    this.editForm = {
      name: user.name,
      roleId: user.roleId,
      branchId: user.branchId ?? null,
      employeeId: user.employeeId ?? null,
      isActive: user.isActive
    };
    this.showEditDialog = true;
  }

  saveEdit() {
    if (this.editUserId == null) return;
    this.saving = true;
    this.adminApi.updateUser(this.editUserId, this.editForm).subscribe({
      next: () => {
        this.saving = false;
        this.showEditDialog = false;
        this.notify.success('User updated successfully.');
        this.loadUsers();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to update user.');
      }
    });
  }

  openResetPasswordDialog(user: UserDto) {
    this.resetPasswordUserId = user.id;
    this.resetPasswordForm = { newPassword: '' };
    this.showResetPasswordDialog = true;
  }

  saveResetPassword() {
    if (this.resetPasswordUserId == null) return;
    this.saving = true;
    this.adminApi.resetUserPassword(this.resetPasswordUserId, this.resetPasswordForm).subscribe({
      next: () => {
        this.saving = false;
        this.showResetPasswordDialog = false;
        this.notify.success('Password reset successfully.');
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to reset password.');
      }
    });
  }

  confirmDeactivate(user: UserDto) {
    this.confirmation.confirm({
      message: `Deactivate user "${user.name}"? They will no longer be able to sign in.`,
      header: 'Confirm Deactivation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deactivate(user)
    });
  }

  deactivate(user: UserDto) {
    this.adminApi.deleteUser(user.id).subscribe({
      next: () => {
        this.notify.success('User deactivated.');
        this.loadUsers();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to deactivate user.')
    });
  }

  private emptyAddForm(): CreateUserDto {
    return { name: '', email: '', password: '', roleId: 0, branchId: null, employeeId: null };
  }

  private emptyEditForm(): UpdateUserDto {
    return { name: '', roleId: 0, branchId: null, employeeId: null, isActive: true };
  }
}

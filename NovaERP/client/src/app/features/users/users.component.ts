import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
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
import { UserApiService, UserDto, CreateUserDto, UpdateUserDto } from '../../core/user-api.service';
import { RoleApiService, RoleDto } from '../../core/role-api.service';
import { BranchApiService, BranchDto } from '../../core/branch-api.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    CommonModule, RouterModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, DropdownModule, TagModule, ConfirmDialogModule, ToastModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent implements OnInit {
  users: UserDto[] = [];
  roles: RoleDto[] = [];
  branches: BranchDto[] = [];
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

  addPhotoPreview: string | null = null;
  editPhotoPreview: string | null = null;
  photoError: string | null = null;

  private readonly maxPhotoBytes = 2 * 1024 * 1024;

  constructor(
    private userApi: UserApiService,
    private roleApi: RoleApiService,
    private branchApi: BranchApiService,
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
    this.userApi.getAll().subscribe({
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
    this.roleApi.getAll().subscribe({
      next: data => (this.roles = data),
      error: () => (this.roles = [])
    });
  }

  loadBranches() {
    this.branchApi.getAll().subscribe({
      next: data => (this.branches = data),
      error: () => (this.branches = [])
    });
  }

  openAddDialog() {
    this.addForm = this.emptyAddForm();
    this.addPhotoPreview = null;
    this.photoError = null;
    this.showAddDialog = true;
  }

  onPhotoSelected(event: Event, mode: 'add' | 'edit') {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.photoError = null;
    if (!file.type.startsWith('image/')) {
      this.photoError = 'Please choose an image file.';
      input.value = '';
      return;
    }
    if (file.size > this.maxPhotoBytes) {
      this.photoError = 'Photo is too large — please use an image under 2 MB.';
      input.value = '';
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      const dataUrl = reader.result as string;
      const [prefix, base64] = dataUrl.split(',');
      const contentType = prefix.slice(prefix.indexOf(':') + 1, prefix.indexOf(';'));

      if (mode === 'add') {
        this.addForm.photoData = base64;
        this.addForm.photoContentType = contentType;
        this.addPhotoPreview = dataUrl;
      } else {
        this.editForm.photoData = base64;
        this.editForm.photoContentType = contentType;
        this.editForm.removePhoto = false;
        this.editPhotoPreview = dataUrl;
      }
    };
    reader.readAsDataURL(file);
    input.value = '';
  }

  roleName(roleId: number | null | undefined): string | null {
    return this.roles.find(r => r.id === roleId)?.name ?? null;
  }

  removePhoto(mode: 'add' | 'edit') {
    this.photoError = null;
    if (mode === 'add') {
      this.addForm.photoData = null;
      this.addForm.photoContentType = null;
      this.addPhotoPreview = null;
    } else {
      this.editForm.photoData = null;
      this.editForm.photoContentType = null;
      this.editForm.removePhoto = true;
      this.editPhotoPreview = null;
    }
  }

  saveAdd() {
    this.saving = true;
    this.userApi.create(this.addForm).subscribe({
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
      isActive: user.isActive,
      photoData: null,
      photoContentType: null,
      removePhoto: false
    };
    this.editPhotoPreview = user.photoUrl ?? null;
    this.photoError = null;
    this.showEditDialog = true;
  }

  saveEdit() {
    if (this.editUserId == null) return;
    this.saving = true;
    this.userApi.update(this.editUserId, this.editForm).subscribe({
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
    this.userApi.resetPassword(this.resetPasswordUserId, this.resetPasswordForm).subscribe({
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
    this.userApi.delete(user.id).subscribe({
      next: () => {
        this.notify.success('User deactivated.');
        this.loadUsers();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to deactivate user.')
    });
  }

  private emptyAddForm(): CreateUserDto {
    return { name: '', email: '', password: '', roleId: 0, branchId: null, photoData: null, photoContentType: null };
  }

  private emptyEditForm(): UpdateUserDto {
    return { name: '', roleId: 0, branchId: null, isActive: true, photoData: null, photoContentType: null, removePhoto: false };
  }
}

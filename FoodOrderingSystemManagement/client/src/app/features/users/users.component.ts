import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { MatIconModule }            from '@angular/material/icon';
import { MatButtonModule }          from '@angular/material/button';
import { MatFormFieldModule }       from '@angular/material/form-field';
import { MatInputModule }           from '@angular/material/input';
import { MatSelectModule }          from '@angular/material/select';
import { MatTooltipModule }         from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatExpansionModule }       from '@angular/material/expansion';
import { MatCheckboxModule }        from '@angular/material/checkbox';
import { UserService }           from '../../core/services/user.service';
import { RoleService }           from '../../core/services/role.service';
import { RolePermissionService } from '../../core/services/role-permission.service';
import { NotificationService }   from '../../shared/services/notification.service';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { AppUser }               from '../../core/models/user.model';
import { UpdateUserRequest }     from '../../core/models/user.model';
import { Role }                  from '../../core/models/role.model';
import { AuthService }           from '../../core/authentication/auth.service';
import { APP_CONSTANTS }         from '../../core/constants/app.constants';
import { APP_FEATURES, FeatureGroup, ALL_FEATURE_KEYS } from '../../core/constants/app-features.constant';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, FormsModule,
    MatIconModule, MatButtonModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatTooltipModule,
    MatProgressSpinnerModule, MatExpansionModule, MatCheckboxModule,
    LoadingSpinnerComponent,
  ],
  templateUrl: './users.component.html',
  styleUrl:    './users.component.scss',
})
export class UsersComponent implements OnInit {
  private userService  = inject(UserService);
  private roleService  = inject(RoleService);
  private permService  = inject(RolePermissionService);
  private authService  = inject(AuthService);
  private notify       = inject(NotificationService);
  private fb           = inject(FormBuilder);

  get isAdmin(): boolean { return this.authService.userRole() === APP_CONSTANTS.ROLES.ADMIN; }

  users        = signal<AppUser[]>([]);
  roles        = signal<Role[]>([]);
  loading      = signal(true);
  showForm     = signal(false);
  saving       = signal(false);
  toggling     = signal<number | null>(null);
  showPassword  = signal(false);
  avatarPreview = signal<string | null>(null);
  editingUser   = signal<AppUser | null>(null);
  get isEditing(): boolean { return this.editingUser() !== null; }

  // Role CRUD
  showRoleInput = signal(false);
  newRoleName   = signal('');
  savingRole    = signal(false);

  // ── Right sheet state ─────────────────────────────────
  sheetOpen            = signal(false);
  selectedRoleForPerms = signal<Role | null>(null);
  selectedPermissions  = signal<Set<string>>(new Set());
  searchPerms          = signal('');
  permLoading          = signal(false);
  permSaving           = signal(false);

  isAdminRoleSel = computed(() => this.selectedRoleForPerms()?.roleName === 'Admin');
  selectedCount  = computed(() => this.selectedPermissions().size);

  readonly featureGroups     = APP_FEATURES;
  readonly totalFeatureCount = ALL_FEATURE_KEYS.length;

  filteredGroups = computed(() => {
    const q = this.searchPerms().toLowerCase().trim();
    if (!q) return this.featureGroups;
    return this.featureGroups
      .map(g => ({ ...g, features: g.features.filter(f => f.label.toLowerCase().includes(q) || g.module.toLowerCase().includes(q)) }))
      .filter(g => g.features.length > 0);
  });

  get isGlobalAllPerms(): boolean   { return this.selectedCount() === this.totalFeatureCount; }
  get isGlobalIndeterminate(): boolean { return this.selectedCount() > 0 && !this.isGlobalAllPerms; }

  // ── Form ───────────────────────────────────────────────
  readonly defaultRoleId = computed(() => this.roles().find(r => r.roleName === 'Cashier')?.id ?? this.roles()[0]?.id ?? 2);

  form = this.fb.group({
    name:     ['', [Validators.required, Validators.maxLength(100)]],
    email:    ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
    password: ['', [Validators.minLength(6)]],
    roleId:   [2, Validators.required],
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    forkJoin({ users: this.userService.getAll(), roles: this.roleService.getAll() }).subscribe({
      next: ({ users, roles }) => {
        this.users.set(users.data ?? []);
        this.roles.set(roles.data ?? []);
        this.loading.set(false);
      },
      error: () => { this.notify.error('Failed to load data.'); this.loading.set(false); },
    });
  }

  openForm(): void {
    this.editingUser.set(null);
    this.form.reset({ roleId: this.defaultRoleId() });
    this.form.get('password')!.addValidators(Validators.required);
    this.form.get('password')!.updateValueAndValidity();
    this.avatarPreview.set(null);
    this.showForm.set(true);
  }

  openEditForm(user: AppUser): void {
    this.editingUser.set(user);
    this.form.reset({ name: user.name, email: user.email, roleId: user.roleId, password: '' });
    this.form.get('password')!.removeValidators(Validators.required);
    this.form.get('password')!.updateValueAndValidity();
    this.avatarPreview.set(user.profileImage ?? null);
    this.showForm.set(true);
  }

  closeForm(): void { this.showForm.set(false); this.avatarPreview.set(null); this.editingUser.set(null); }

  onAvatarChange(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    if (!file.type.startsWith('image/')) { this.notify.warn('Please select an image file.'); return; }
    if (file.size > 2 * 1024 * 1024) { this.notify.warn('Image must be under 2 MB.'); return; }
    const reader = new FileReader();
    reader.onload = () => this.avatarPreview.set(reader.result as string);
    reader.readAsDataURL(file);
  }

  removeAvatar(): void { this.avatarPreview.set(null); }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    const editing = this.editingUser();
    if (editing) {
      const payload: UpdateUserRequest = {
        name: this.form.value.name!,
        email: this.form.value.email!,
        roleId: this.form.value.roleId!,
        profileImage: this.avatarPreview() ?? null,
      };
      this.userService.update(editing.id, payload).subscribe({
        next: () => { this.saving.set(false); this.notify.success('User updated successfully.'); this.closeForm(); this.load(); },
        error: err => { this.saving.set(false); this.notify.error(err?.error?.message || 'Failed to update user.'); },
      });
    } else {
      const payload: any = { ...this.form.value, profileImage: this.avatarPreview() ?? null };
      this.userService.create(payload).subscribe({
        next: () => { this.saving.set(false); this.notify.success('User created successfully.'); this.closeForm(); this.load(); },
        error: err => { this.saving.set(false); this.notify.error(err?.error?.message || 'Failed to create user.'); },
      });
    }
  }

  toggleActive(user: AppUser): void {
    this.toggling.set(user.id);
    this.userService.toggleActive(user.id).subscribe({
      next: () => { this.toggling.set(null); this.notify.success(`${user.name} ${user.isActive ? 'deactivated' : 'activated'}.`); this.load(); },
      error: err => { this.toggling.set(null); this.notify.error(err?.error?.message || 'Failed to update user.'); },
    });
  }

  openRoleInput(): void  { this.newRoleName.set(''); this.showRoleInput.set(true); }
  cancelRoleInput(): void { this.showRoleInput.set(false); }

  saveRole(): void {
    const name = this.newRoleName().trim();
    if (!name) return;
    this.savingRole.set(true);
    this.roleService.create({ roleName: name }).subscribe({
      next: res => {
        this.savingRole.set(false); this.showRoleInput.set(false);
        this.notify.success(`Role '${res.data?.roleName}' created.`);
        this.roleService.getAll().subscribe(r => this.roles.set(r.data ?? []));
      },
      error: err => { this.savingRole.set(false); this.notify.error(err?.error?.message || 'Failed to create role.'); },
    });
  }

  // ── Right sheet ────────────────────────────────────────
  openSheet(role: Role): void {
    this.selectedRoleForPerms.set(role);
    this.searchPerms.set('');
    this.sheetOpen.set(true);

    if (role.roleName === 'Admin') {
      this.selectedPermissions.set(new Set(ALL_FEATURE_KEYS));
      return;
    }
    this.permLoading.set(true);
    this.permService.getForRole(role.id).subscribe({
      next: res => { this.selectedPermissions.set(new Set(res.data ?? [])); this.permLoading.set(false); },
      error: () => this.permLoading.set(false),
    });
  }

  closeSheet(): void {
    this.sheetOpen.set(false);
    setTimeout(() => { if (!this.sheetOpen()) this.selectedRoleForPerms.set(null); }, 320);
  }

  togglePerm(key: string): void {
    if (this.isAdminRoleSel()) return;
    const next = new Set(this.selectedPermissions());
    if (next.has(key)) next.delete(key); else next.add(key);
    this.selectedPermissions.set(next);
  }

  isGroupAllPerms(g: FeatureGroup): boolean {
    return g.features.every(f => this.selectedPermissions().has(f.key));
  }
  isGroupIndPerms(g: FeatureGroup): boolean {
    const c = g.features.filter(f => this.selectedPermissions().has(f.key)).length;
    return c > 0 && c < g.features.length;
  }

  toggleGroupPerms(g: FeatureGroup): void {
    if (this.isAdminRoleSel()) return;
    const all = this.isGroupAllPerms(g);
    const next = new Set(this.selectedPermissions());
    g.features.forEach(f => all ? next.delete(f.key) : next.add(f.key));
    this.selectedPermissions.set(next);
  }

  toggleGlobalPerms(): void {
    if (this.isAdminRoleSel()) return;
    this.selectedPermissions.set(this.isGlobalAllPerms ? new Set() : new Set(ALL_FEATURE_KEYS));
  }

  getGroupPermCount(g: FeatureGroup): number {
    return g.features.filter(f => this.selectedPermissions().has(f.key)).length;
  }

  savePermissions(): void {
    const role = this.selectedRoleForPerms();
    if (!role) return;
    this.permSaving.set(true);
    this.permService.saveForRole(role.id, Array.from(this.selectedPermissions())).subscribe({
      next: () => { this.permSaving.set(false); this.notify.success(`Permissions saved for "${role.roleName}".`); },
      error: err => { this.permSaving.set(false); this.notify.error(err?.error?.message || 'Failed to save permissions.'); },
    });
  }

  getRoleIcon(n: string): string {
    const l = n.toLowerCase();
    if (l === 'admin')            return 'admin_panel_settings';
    if (l === 'cashier')          return 'point_of_sale';
    if (l === 'waiter')           return 'room_service';
    if (l === 'inventorymanager') return 'inventory_2';
    return 'badge';
  }

  getRoleClass(n: string): string { return n.toLowerCase().replace(/\s+/g, ''); }
}

import { Component, signal, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken } from '../../core/auth.helper';
import { HasPermissionDirective } from '../../core/permission.directive';

interface Role { id: number; name: string; isSystemRole: boolean; permissionKeys: string[]; }
interface Permission { id: number; key: string; module: string; description?: string; }
interface ModuleGroup { module: string; permissions: Permission[]; }

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [FormsModule, HasPermissionDirective],
  templateUrl: './roles.component.html',
  styleUrl: './roles.component.scss'
})
export class RolesComponent implements OnInit {
  roles = signal<Role[]>([]);
  moduleGroups = signal<ModuleGroup[]>([]);
  loading  = signal(false);
  showForm = signal(false);
  editId   = signal<number | null>(null);

  orgId = getOrgIdFromToken();
  name = '';
  selectedKeys = new Set<string>();

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadRoles();
    this.loadPermissions();
  }

  private authHeaders() {
    return { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` };
  }

  loadRoles() {
    this.loading.set(true);
    this.http.get<Role[]>(`${environment.apiUrl}/roles/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe({
      next: data => { this.roles.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  loadPermissions() {
    this.http.get<Permission[]>(`${environment.apiUrl}/permissions`, {
      headers: this.authHeaders()
    }).subscribe(data => {
      const byModule = new Map<string, Permission[]>();
      for (const p of data) {
        if (!byModule.has(p.module)) byModule.set(p.module, []);
        byModule.get(p.module)!.push(p);
      }
      this.moduleGroups.set(Array.from(byModule.entries()).map(([module, permissions]) => ({ module, permissions })));
    });
  }

  openAdd() {
    this.editId.set(null);
    this.name = '';
    this.selectedKeys = new Set<string>();
    this.showForm.set(true);
  }

  openEdit(r: Role) {
    if (r.isSystemRole) return;
    this.editId.set(r.id);
    this.name = r.name;
    this.selectedKeys = new Set(r.permissionKeys);
    this.showForm.set(true);
  }

  isModuleFullySelected(group: ModuleGroup): boolean {
    return group.permissions.length > 0 && group.permissions.every(p => this.selectedKeys.has(p.key));
  }

  isModulePartiallySelected(group: ModuleGroup): boolean {
    const some = group.permissions.some(p => this.selectedKeys.has(p.key));
    return some && !this.isModuleFullySelected(group);
  }

  toggleModule(group: ModuleGroup) {
    const shouldSelect = !this.isModuleFullySelected(group);
    for (const p of group.permissions) {
      if (shouldSelect) this.selectedKeys.add(p.key);
      else this.selectedKeys.delete(p.key);
    }
  }

  togglePermission(key: string) {
    if (this.selectedKeys.has(key)) this.selectedKeys.delete(key);
    else this.selectedKeys.add(key);
  }

  save() {
    const dto = { name: this.name, permissionKeys: Array.from(this.selectedKeys) };
    const id = this.editId();
    const req$ = id
      ? this.http.put(`${environment.apiUrl}/roles/${id}`, dto, { headers: this.authHeaders() })
      : this.http.post(`${environment.apiUrl}/roles/org/${this.orgId}`, dto, { headers: this.authHeaders() });
    req$.subscribe(() => { this.showForm.set(false); this.loadRoles(); });
  }

  delete(r: Role) {
    if (r.isSystemRole) return;
    if (!confirm(`Delete role "${r.name}"?`)) return;
    this.http.delete(`${environment.apiUrl}/roles/${r.id}`, { headers: this.authHeaders() })
      .subscribe(() => this.loadRoles());
  }
}

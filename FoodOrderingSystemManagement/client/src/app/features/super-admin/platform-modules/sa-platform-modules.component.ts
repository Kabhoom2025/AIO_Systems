import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialogModule } from '@angular/material/dialog';
import { PlatformModuleService } from '../../../core/services/platform-module.service';
import { PlatformModule, OrgModuleStatus, OrgModuleItem } from '../../../core/models/platform-module.model';

type Tab = 'modules' | 'organizations';

@Component({
  selector: 'app-sa-platform-modules',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, MatTooltipModule, MatSnackBarModule, MatDialogModule],
  templateUrl: './sa-platform-modules.component.html',
  styleUrl: './sa-platform-modules.component.scss',
})
export class SaPlatformModulesComponent implements OnInit {
  private readonly svc   = inject(PlatformModuleService);
  private readonly snack = inject(MatSnackBar);
  private readonly fb    = inject(FormBuilder);

  activeTab      = signal<Tab>('modules');
  modules        = signal<PlatformModule[]>([]);
  orgStatuses    = signal<OrgModuleStatus[]>([]);
  loading        = signal(false);
  showForm       = signal(false);
  editingId      = signal<number | null>(null);
  savingOrgId    = signal<number | null>(null);

  form = this.fb.group({
    name:        ['', [Validators.required, Validators.maxLength(100)]],
    key:         ['', [Validators.required, Validators.maxLength(50), Validators.pattern(/^[a-z0-9_]+$/)]],
    description: ['', Validators.maxLength(500)],
    icon:        ['widgets', Validators.required],
    color:       ['#6c757d'],
    sortOrder:   [0],
  });

  get isEditing(): boolean { return this.editingId() !== null; }

  ngOnInit(): void {
    this.loadModules();
    this.loadOrgStatuses();
  }

  setTab(tab: Tab): void { this.activeTab.set(tab); }

  loadModules(): void {
    this.loading.set(true);
    this.svc.getAll().subscribe({
      next:  m  => { this.modules.set(m); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  loadOrgStatuses(): void {
    this.svc.getAllOrgStatus().subscribe({ next: s => this.orgStatuses.set(s) });
  }

  openCreate(): void {
    this.editingId.set(null);
    this.form.reset({ icon: 'widgets', color: '#6c757d', sortOrder: 0 });
    this.form.get('key')?.enable();
    this.showForm.set(true);
  }

  openEdit(mod: PlatformModule): void {
    this.editingId.set(mod.id);
    this.form.patchValue({
      name: mod.name, key: mod.key, description: mod.description,
      icon: mod.icon, color: mod.color, sortOrder: mod.sortOrder,
    });
    this.form.get('key')?.disable();
    this.showForm.set(true);
  }

  cancelForm(): void { this.showForm.set(false); this.editingId.set(null); }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.getRawValue() as any;
    if (this.isEditing) {
      this.svc.update(this.editingId()!, { ...v, isActive: true }).subscribe({
        next: () => { this.toast('Module updated'); this.cancelForm(); this.loadModules(); },
        error: e  => this.toast(e.error?.message ?? 'Failed to update', true),
      });
    } else {
      this.svc.create(v).subscribe({
        next: () => { this.toast('Module created'); this.cancelForm(); this.loadModules(); },
        error: e  => this.toast(e.error?.message ?? 'Failed to create', true),
      });
    }
  }

  toggleActive(mod: PlatformModule): void {
    this.svc.update(mod.id, {
      name: mod.name, description: mod.description, icon: mod.icon,
      color: mod.color, sortOrder: mod.sortOrder, isActive: !mod.isActive,
    }).subscribe({
      next: () => { this.toast(`Module ${!mod.isActive ? 'activated' : 'deactivated'}`); this.loadModules(); },
      error: e  => this.toast(e.error?.message ?? 'Failed', true),
    });
  }

  deleteModule(mod: PlatformModule): void {
    if (!confirm(`Delete module "${mod.name}"? This will remove it from all organizations.`)) return;
    this.svc.delete(mod.id).subscribe({
      next: () => { this.toast('Module deleted'); this.loadModules(); this.loadOrgStatuses(); },
      error: e  => this.toast(e.error?.message ?? 'Failed to delete', true),
    });
  }

  toggleOrgModule(org: OrgModuleStatus, item: OrgModuleItem): void {
    item.isEnabled = !item.isEnabled;
  }

  saveOrgModules(org: OrgModuleStatus): void {
    this.savingOrgId.set(org.organizationId);
    const enabledIds = org.modules.filter(m => m.isEnabled).map(m => m.moduleId);
    this.svc.assignModules({ organizationId: org.organizationId, moduleIds: enabledIds }).subscribe({
      next: () => { this.savingOrgId.set(null); this.toast(`Modules updated for ${org.organizationName}`); },
      error: e  => { this.savingOrgId.set(null); this.toast(e.error?.message ?? 'Failed', true); },
    });
  }

  countEnabled(org: OrgModuleStatus): number {
    return org.modules.filter(m => m.isEnabled).length;
  }

  private toast(msg: string, isError = false): void {
    this.snack.open(msg, 'Close', {
      duration: 3000,
      panelClass: isError ? ['snack-error'] : ['snack-success'],
    });
  }
}

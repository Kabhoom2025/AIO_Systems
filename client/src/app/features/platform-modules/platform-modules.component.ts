import { Component, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { PlatformModuleService } from '../../core/services/platform-module.service';
import { PlatformModule } from '../../core/models/platform-module.model';
import { OrgModuleStatus } from '../../core/models/platform-module.model';
import {
  PlatformModuleFormDialogComponent,
  PlatformModuleFormResult,
} from './platform-module-form-dialog.component';

@Component({
  selector: 'app-platform-modules',
  imports: [
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    MatTooltipModule,
    MatSlideToggleModule,
  ],
  templateUrl: './platform-modules.component.html',
  styleUrl: './platform-modules.component.scss',
})
export class PlatformModulesComponent implements OnInit {
  loading = signal(true);
  modules = signal<PlatformModule[]>([]);
  displayedColumns = ['name', 'key', 'description', 'isActive', 'sortOrder', 'actions'];

  matrixLoaded = signal(false);
  matrix = signal<OrgModuleStatus[]>([]);

  constructor(private moduleService: PlatformModuleService, private dialog: MatDialog) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.moduleService.getAll().subscribe({
      next: (modules) => {
        this.modules.set(modules);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  loadMatrix(): void {
    if (this.matrixLoaded()) return;
    this.moduleService.getAllOrgStatus().subscribe((matrix) => {
      this.matrix.set(matrix);
      this.matrixLoaded.set(true);
    });
  }

  openCreateDialog(): void {
    const ref = this.dialog.open(PlatformModuleFormDialogComponent, { width: '480px' });
    ref.afterClosed().subscribe((result: PlatformModuleFormResult | undefined) => {
      if (!result) return;
      this.moduleService.create(result).subscribe(() => this.load());
    });
  }

  openEditDialog(module: PlatformModule): void {
    const ref = this.dialog.open(PlatformModuleFormDialogComponent, {
      width: '480px',
      data: { module },
    });
    ref.afterClosed().subscribe((result: PlatformModuleFormResult | undefined) => {
      if (!result) return;
      this.moduleService.update(module.id, result).subscribe(() => this.load());
    });
  }

  deleteModule(module: PlatformModule): void {
    if (!confirm(`Delete module "${module.name}"?`)) return;
    this.moduleService.delete(module.id).subscribe(() => this.load());
  }

  toggleAssignment(org: OrgModuleStatus, moduleId: number, enabled: boolean): void {
    const currentlyEnabledIds = org.modules.filter((m) => m.isEnabled).map((m) => m.moduleId);
    const nextIds = enabled
      ? [...currentlyEnabledIds, moduleId]
      : currentlyEnabledIds.filter((id) => id !== moduleId);

    this.moduleService
      .assignToOrg({ organizationId: org.organizationId, moduleIds: nextIds })
      .subscribe(() => {
        this.matrixLoaded.set(false);
        this.loadMatrix();
      });
  }
}

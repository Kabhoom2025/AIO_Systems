import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  WorkflowDefinitionApiService,
  WorkflowDefinitionDto,
  CreateWorkflowDefinitionDto,
  CreateWorkflowStepDefinitionDto
} from '../../../core/workflow-definition-api.service';
import { RoleApiService, RoleDto } from '../../../core/role-api.service';

interface CanvasStep {
  tempId: string;
  stepOrder: number;
  name: string;
  approverRoleId: number | null;
  minAmount: number | null;
}

let tempIdCounter = 0;

@Component({
  selector: 'app-workflow-canvas',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ButtonModule, InputTextModule,
    InputNumberModule, DropdownModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './workflow-canvas.component.html',
  styleUrl: './workflow-canvas.component.scss'
})
export class WorkflowCanvasComponent implements OnInit {
  definitionId: number | null = null;
  loading = false;
  saving = false;

  name = '';
  entityType = '';
  isActive = true;
  steps: CanvasStep[] = [];
  roles: RoleDto[] = [];

  viewOnly = false;

  selectedIndex: number | null = null;
  panelClosing = false;

  openMenuAt: number | null = null;
  menuClosing = false;

  removingIds = new Set<string>();

  zoom = 1;
  readonly minZoom = 0.5;
  readonly maxZoom = 1.5;

  savedSnapshot = '';
  savedAt: Date | null = null;
  savedAgoLabel = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private api: WorkflowDefinitionApiService,
    private roleApi: RoleApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.roleApi.getAll().subscribe({ next: rows => (this.roles = rows), error: () => (this.roles = []) });

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.definitionId = Number(idParam);
      this.loading = true;
      this.api.getById(this.definitionId).subscribe({
        next: def => this.hydrate(def),
        error: err => {
          this.loading = false;
          this.notify.error(err.error?.message ?? 'Failed to load workflow definition.');
        }
      });
    } else {
      this.snapshotSaved();
    }

    setInterval(() => this.refreshSavedAgoLabel(), 15000);
  }

  private hydrate(def: WorkflowDefinitionDto): void {
    this.name = def.name;
    this.entityType = def.entityType;
    this.isActive = def.isActive;
    this.steps = def.steps
      .slice()
      .sort((a, b) => a.stepOrder - b.stepOrder)
      .map(s => ({
        tempId: `s${tempIdCounter++}`,
        stepOrder: s.stepOrder,
        name: s.name,
        approverRoleId: s.approverRoleId ?? null,
        minAmount: s.minAmount ?? null
      }));
    this.loading = false;
    this.snapshotSaved();
  }

  private currentPayload(): CreateWorkflowDefinitionDto {
    return {
      name: this.name,
      entityType: this.entityType,
      isActive: this.isActive,
      steps: this.steps.map((s, i): CreateWorkflowStepDefinitionDto => ({
        stepOrder: i + 1,
        name: s.name,
        approverRoleId: s.approverRoleId,
        minAmount: s.minAmount
      }))
    };
  }

  private snapshotSaved(): void {
    this.savedSnapshot = JSON.stringify(this.currentPayload());
    this.savedAt = new Date();
    this.refreshSavedAgoLabel();
  }

  private refreshSavedAgoLabel(): void {
    if (!this.savedAt) { this.savedAgoLabel = ''; return; }
    const seconds = Math.round((new Date().getTime() - this.savedAt.getTime()) / 1000);
    if (seconds < 10) this.savedAgoLabel = 'Saved just now';
    else if (seconds < 60) this.savedAgoLabel = `Saved ${seconds}s ago`;
    else this.savedAgoLabel = `Saved ${Math.round(seconds / 60)}m ago`;
  }

  get isDirty(): boolean {
    return JSON.stringify(this.currentPayload()) !== this.savedSnapshot;
  }

  approverRoleName(id: number | null): string | null {
    if (id == null) return null;
    return this.roles.find(r => r.id === id)?.name ?? null;
  }

  selectStep(index: number): void {
    if (this.selectedIndex === index) return;
    this.selectedIndex = index;
    this.panelClosing = false;
  }

  closePanel(): void {
    this.panelClosing = true;
    setTimeout(() => {
      this.selectedIndex = null;
      this.panelClosing = false;
    }, 200);
  }

  toggleInsertMenu(position: number): void {
    if (this.openMenuAt === position) {
      this.closeInsertMenu();
      return;
    }
    this.openMenuAt = position;
    this.menuClosing = false;
  }

  closeInsertMenu(): void {
    if (this.openMenuAt === null) return;
    this.menuClosing = true;
    setTimeout(() => {
      this.openMenuAt = null;
      this.menuClosing = false;
    }, 150);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (this.openMenuAt === null) return;
    const target = event.target as HTMLElement;
    if (!target.closest('.connector')) this.closeInsertMenu();
  }

  addStepAt(position: number): void {
    const step: CanvasStep = {
      tempId: `s${tempIdCounter++}`,
      stepOrder: position + 1,
      name: '',
      approverRoleId: null,
      minAmount: null
    };
    this.steps.splice(position, 0, step);
    this.renumber();
    this.closeInsertMenu();
    this.selectedIndex = position;
    this.panelClosing = false;
  }

  private renumber(): void {
    this.steps.forEach((s, i) => (s.stepOrder = i + 1));
  }

  removeStep(index: number): void {
    const step = this.steps[index];
    this.confirm.confirm({
      message: `Remove step "${step.name || 'Untitled step'}"?`,
      header: 'Confirm Remove',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.removingIds.add(step.tempId);
        setTimeout(() => {
          const idx = this.steps.findIndex(s => s.tempId === step.tempId);
          if (idx !== -1) this.steps.splice(idx, 1);
          this.removingIds.delete(step.tempId);
          this.renumber();
          if (this.selectedIndex !== null) this.closePanel();
        }, 220);
      }
    });
  }

  isRemoving(step: CanvasStep): boolean {
    return this.removingIds.has(step.tempId);
  }

  zoomIn(): void {
    this.zoom = Math.min(this.maxZoom, Math.round((this.zoom + 0.1) * 10) / 10);
  }

  zoomOut(): void {
    this.zoom = Math.max(this.minZoom, Math.round((this.zoom - 0.1) * 10) / 10);
  }

  resetZoom(): void {
    this.zoom = 1;
  }

  toggleViewOnly(): void {
    this.viewOnly = !this.viewOnly;
    if (this.viewOnly) this.closeInsertMenu();
  }

  back(): void {
    this.router.navigate(['/workflow-definitions']);
  }

  save(): void {
    if (!this.name || !this.entityType) {
      this.notify.warn('Name and entity type are required.');
      return;
    }
    if (this.steps.some(s => !s.name)) {
      this.notify.warn('Every step needs a name.');
      return;
    }
    this.saving = true;
    const payload = this.currentPayload();
    const req$ = this.definitionId
      ? this.api.update(this.definitionId, payload)
      : this.api.create(payload);
    req$.subscribe({
      next: def => {
        this.saving = false;
        this.snapshotSaved();
        this.notify.success(`Workflow definition ${this.definitionId ? 'updated' : 'created'}.`);
        if (!this.definitionId) {
          this.definitionId = def.id;
          this.router.navigate(['/workflow-definitions', def.id], { replaceUrl: true });
        }
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save workflow definition.');
      }
    });
  }
}

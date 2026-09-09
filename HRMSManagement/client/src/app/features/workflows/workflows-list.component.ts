import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

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
import {
  WorkflowApiService,
  WorkflowDefinitionDto,
  CreateWorkflowDefinitionDto,
  TriggerTypeDto
} from '../../core/workflow-api.service';

@Component({
  selector: 'app-workflows-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    InputTextarea,
    DropdownModule,
    TagModule,
    ToastModule,
    ConfirmDialogModule,
    HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './workflows-list.component.html',
  styleUrl: './workflows-list.component.scss'
})
export class WorkflowsListComponent implements OnInit {
  workflows: WorkflowDefinitionDto[] = [];
  loading = false;

  triggerTypes: TriggerTypeDto[] = [];

  showCreateDialog = false;
  saving = false;
  createForm: CreateWorkflowDefinitionDto = this.emptyForm();

  constructor(
    private api: WorkflowApiService,
    private router: Router,
    private notify: NotificationService,
    private confirmation: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.loadWorkflows();
    this.loadTriggerTypes();
  }

  loadWorkflows() {
    this.loading = true;
    this.api.getWorkflows().subscribe({
      next: data => {
        this.workflows = data;
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load workflows.');
      }
    });
  }

  loadTriggerTypes() {
    this.api.getTriggerTypes().subscribe({
      next: data => (this.triggerTypes = data),
      error: () => (this.triggerTypes = [])
    });
  }

  openCreateDialog() {
    this.createForm = this.emptyForm();
    this.showCreateDialog = true;
  }

  saveWorkflow() {
    if (!this.createForm.name || !this.createForm.triggerType) {
      this.notify.warn('Please fill in name and trigger.');
      return;
    }
    this.saving = true;
    this.api.createWorkflow(this.createForm).subscribe({
      next: wf => {
        this.saving = false;
        this.showCreateDialog = false;
        this.notify.success('Workflow created — configure it in the builder.');
        this.router.navigate(['/workflows', wf.id]);
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to create workflow.');
      }
    });
  }

  openBuilder(wf: WorkflowDefinitionDto) {
    this.router.navigate(['/workflows', wf.id]);
  }

  confirmDelete(wf: WorkflowDefinitionDto) {
    this.confirmation.confirm({
      message: `Delete workflow "${wf.name}"? This cannot be undone.`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deleteWorkflow(wf)
    });
  }

  deleteWorkflow(wf: WorkflowDefinitionDto) {
    this.api.deleteWorkflow(wf.id).subscribe({
      next: () => {
        this.notify.success('Workflow deleted.');
        this.loadWorkflows();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to delete workflow.')
    });
  }

  publishStatus(wf: WorkflowDefinitionDto): { label: string; severity: 'success' | 'warn' | 'info' } {
    if (!wf.publishedVersionId) {
      return { label: 'Draft only', severity: 'warn' };
    }
    if (wf.publishedVersionNumber === wf.draftVersionNumber) {
      return { label: 'Published', severity: 'success' };
    }
    return { label: 'Published (older)', severity: 'info' };
  }

  private emptyForm(): CreateWorkflowDefinitionDto {
    return { name: '', description: '', triggerType: '' };
  }
}

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import { WorkflowInstanceApiService, WorkflowInstanceDto } from '../../core/workflow-instance-api.service';

@Component({
  selector: 'app-workflow-instances',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    TagModule, TextareaModule, ToastModule, HasPermissionDirective
  ],
  templateUrl: './workflow-instances.component.html',
  styleUrl: './workflow-instances.component.scss'
})
export class WorkflowInstancesComponent implements OnInit {
  instances: WorkflowInstanceDto[] = [];
  loading = false;

  showDetail = false;
  selected: WorkflowInstanceDto | null = null;

  showActionDialog = false;
  actionMode: 'approve' | 'reject' | null = null;
  actionTarget: WorkflowInstanceDto | null = null;
  actionComments = '';
  actioning = false;

  constructor(
    private api: WorkflowInstanceApiService,
    private notify: NotificationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.api.getPending().subscribe({
      next: rows => { this.instances = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load pending approvals.');
      }
    });
  }

  viewDetail(instance: WorkflowInstanceDto) {
    this.selected = instance;
    this.showDetail = true;
  }

  openAction(instance: WorkflowInstanceDto, mode: 'approve' | 'reject') {
    this.actionTarget = instance;
    this.actionMode = mode;
    this.actionComments = '';
    this.showActionDialog = true;
  }

  confirmAction() {
    if (!this.actionTarget || !this.actionMode) return;
    this.actioning = true;
    const dto = { comments: this.actionComments || null };
    const req$ = this.actionMode === 'approve'
      ? this.api.approve(this.actionTarget.id, dto)
      : this.api.reject(this.actionTarget.id, dto);
    req$.subscribe({
      next: () => {
        this.actioning = false;
        this.showActionDialog = false;
        this.showDetail = false;
        this.notify.success(`Workflow instance ${this.actionMode === 'approve' ? 'approved' : 'rejected'}.`);
        this.load();
      },
      error: err => {
        this.actioning = false;
        this.notify.error(err.error?.message ?? `Failed to ${this.actionMode} workflow instance.`);
      }
    });
  }

  statusSeverity(status: string): string {
    switch (status) {
      case 'Approved': return 'success';
      case 'Rejected': return 'danger';
      case 'Pending': return 'warn';
      default: return 'info';
    }
  }
}

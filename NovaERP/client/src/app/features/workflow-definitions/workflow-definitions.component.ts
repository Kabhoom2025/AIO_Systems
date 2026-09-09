import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import { WorkflowDefinitionApiService, WorkflowDefinitionDto } from '../../core/workflow-definition-api.service';

@Component({
  selector: 'app-workflow-definitions',
  standalone: true,
  imports: [
    CommonModule, TableModule, ButtonModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './workflow-definitions.component.html',
  styleUrl: './workflow-definitions.component.scss'
})
export class WorkflowDefinitionsComponent implements OnInit {
  definitions: WorkflowDefinitionDto[] = [];
  loading = false;

  constructor(
    private api: WorkflowDefinitionApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.definitions = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load workflow definitions.');
      }
    });
  }

  openNew() {
    this.router.navigate(['/workflow-definitions', 'new']);
  }

  openEdit(def: WorkflowDefinitionDto) {
    this.router.navigate(['/workflow-definitions', def.id]);
  }

  delete(def: WorkflowDefinitionDto) {
    this.confirm.confirm({
      message: `Delete workflow definition "${def.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(def.id).subscribe({
          next: () => {
            this.notify.success('Workflow definition deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete workflow definition.')
        });
      }
    });
  }
}

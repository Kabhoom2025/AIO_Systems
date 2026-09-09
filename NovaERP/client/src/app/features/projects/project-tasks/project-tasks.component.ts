import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  ProjectTaskApiService, ProjectTaskDto, CreateProjectTaskDto, UpdateProjectTaskDto
} from '../../../core/project-task-api.service';
import { ProjectApiService, ProjectDto } from '../../../core/project-api.service';
import { EmployeeApiService, EmployeeDto } from '../../../core/employee-api.service';

@Component({
  selector: 'app-project-tasks',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, DropdownModule, CalendarModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './project-tasks.component.html',
  styleUrl: './project-tasks.component.scss'
})
export class ProjectTasksComponent implements OnInit {
  tasks: ProjectTaskDto[] = [];
  projects: ProjectDto[] = [];
  employees: EmployeeDto[] = [];
  loading = false;

  priorities = ['Low', 'Medium', 'High'];
  statuses = ['ToDo', 'InProgress', 'Done', 'Blocked'];

  showDialog = false;
  editing: ProjectTaskDto | null = null;
  saving = false;

  form: {
    projectId: number | null;
    assignedToId: number | null;
    title: string;
    description: string | null;
    priority: string;
    status: string;
    startDate: Date | null;
    dueDate: Date | null;
  } = this.emptyForm();

  constructor(
    private api: ProjectTaskApiService,
    private projectApi: ProjectApiService,
    private employeeApi: EmployeeApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.projectApi.getAll().subscribe({ next: rows => (this.projects = rows), error: () => (this.projects = []) });
    this.employeeApi.getAll().subscribe({ next: rows => (this.employees = rows), error: () => (this.employees = []) });
  }

  private emptyForm() {
    return {
      projectId: null, assignedToId: null, title: '', description: null,
      priority: 'Medium', status: 'ToDo', startDate: null, dueDate: null
    };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.tasks = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load tasks.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(task: ProjectTaskDto) {
    this.editing = task;
    this.form = {
      projectId: task.projectId, assignedToId: task.assignedToId,
      title: task.title, description: task.description,
      priority: task.priority, status: task.status,
      startDate: task.startDate ? new Date(task.startDate) : null,
      dueDate: task.dueDate ? new Date(task.dueDate) : null
    };
    this.showDialog = true;
  }

  save() {
    if ((!this.editing && !this.form.projectId) || !this.form.title) {
      this.notify.warn('Project and title are required.');
      return;
    }
    this.saving = true;
    const sharedFields = {
      assignedToId: this.form.assignedToId,
      title: this.form.title,
      description: this.form.description,
      priority: this.form.priority,
      status: this.form.status,
      startDate: this.form.startDate ? this.form.startDate.toISOString() : null,
      dueDate: this.form.dueDate ? this.form.dueDate.toISOString() : null
    };
    const req$ = this.editing
      ? this.api.update(this.editing.id, sharedFields as UpdateProjectTaskDto)
      : this.api.create({ projectId: this.form.projectId!, ...sharedFields } as CreateProjectTaskDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Task ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save task.');
      }
    });
  }

  delete(task: ProjectTaskDto) {
    this.confirm.confirm({
      message: `Delete task "${task.title}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(task.id).subscribe({
          next: () => {
            this.notify.success('Task deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete task.')
        });
      }
    });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Done') return 'success';
    if (status === 'Blocked') return 'danger';
    if (status === 'InProgress') return 'info';
    return 'warn';
  }

  prioritySeverity(priority: string): 'success' | 'danger' | 'info' | 'warn' {
    if (priority === 'High') return 'danger';
    if (priority === 'Medium') return 'warn';
    return 'info';
  }
}

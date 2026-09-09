import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  ProjectApiService, ProjectDto, CreateProjectDto, UpdateProjectDto
} from '../../../core/project-api.service';
import { EmployeeApiService, EmployeeDto } from '../../../core/employee-api.service';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, DropdownModule, CalendarModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.scss'
})
export class ProjectsComponent implements OnInit {
  projects: ProjectDto[] = [];
  employees: EmployeeDto[] = [];
  loading = false;

  statuses = ['Planning', 'Active', 'OnHold', 'Completed', 'Cancelled'];

  showDialog = false;
  editing: ProjectDto | null = null;
  saving = false;

  form: {
    code: string;
    name: string;
    description: string | null;
    managerId: number | null;
    startDate: Date | null;
    endDate: Date | null;
    budget: number | null;
    status: string;
  } = this.emptyForm();

  constructor(
    private api: ProjectApiService,
    private employeeApi: EmployeeApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.employeeApi.getAll().subscribe({ next: rows => (this.employees = rows), error: () => (this.employees = []) });
  }

  private emptyForm() {
    return {
      code: '', name: '', description: null, managerId: null,
      startDate: new Date(), endDate: null, budget: null, status: 'Planning'
    };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.projects = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load projects.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(project: ProjectDto) {
    this.editing = project;
    this.form = {
      code: project.code, name: project.name, description: project.description,
      managerId: project.managerId, startDate: new Date(project.startDate),
      endDate: project.endDate ? new Date(project.endDate) : null,
      budget: project.budget, status: project.status
    };
    this.showDialog = true;
  }

  save() {
    if ((!this.editing && !this.form.code) || !this.form.name || !this.form.managerId) {
      this.notify.warn('Code, name, and manager are required.');
      return;
    }
    this.saving = true;
    const sharedFields = {
      name: this.form.name,
      description: this.form.description,
      managerId: this.form.managerId,
      startDate: (this.form.startDate ?? new Date()).toISOString(),
      endDate: this.form.endDate ? this.form.endDate.toISOString() : null,
      budget: this.form.budget
    };
    const req$ = this.editing
      ? this.api.update(this.editing.id, { ...sharedFields, status: this.form.status } as UpdateProjectDto)
      : this.api.create({ code: this.form.code, ...sharedFields } as CreateProjectDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Project ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save project.');
      }
    });
  }

  delete(project: ProjectDto) {
    this.confirm.confirm({
      message: `Delete project "${project.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(project.id).subscribe({
          next: () => {
            this.notify.success('Project deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete project.')
        });
      }
    });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Completed') return 'success';
    if (status === 'Cancelled') return 'danger';
    if (status === 'Active') return 'info';
    if (status === 'OnHold') return 'warn';
    return 'info';
  }
}

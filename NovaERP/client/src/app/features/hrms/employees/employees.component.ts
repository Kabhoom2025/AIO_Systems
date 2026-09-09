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
  EmployeeApiService, EmployeeDto, CreateEmployeeDto, UpdateEmployeeDto
} from '../../../core/employee-api.service';
import { DepartmentApiService, DepartmentDto } from '../../../core/department-api.service';
import { UserApiService, UserDto } from '../../../core/user-api.service';

@Component({
  selector: 'app-employees',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, DropdownModule, CalendarModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './employees.component.html',
  styleUrl: './employees.component.scss'
})
export class EmployeesComponent implements OnInit {
  employees: EmployeeDto[] = [];
  departments: DepartmentDto[] = [];
  users: UserDto[] = [];
  loading = false;

  employmentTypes = ['Full-Time', 'Part-Time', 'Contract', 'Intern'];

  showDialog = false;
  editing: EmployeeDto | null = null;
  saving = false;

  form: {
    departmentId: number | null;
    userId: number | null;
    reportingManagerId: number | null;
    firstName: string;
    lastName: string;
    email: string;
    phone: string | null;
    dateOfBirth: Date | null;
    gender: string | null;
    jobTitle: string;
    employmentType: string;
    dateOfJoining: Date | null;
  } = this.emptyForm();

  constructor(
    private api: EmployeeApiService,
    private departmentApi: DepartmentApiService,
    private userApi: UserApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.departmentApi.getAll().subscribe({ next: rows => (this.departments = rows), error: () => (this.departments = []) });
    this.userApi.getAll().subscribe({ next: rows => (this.users = rows), error: () => (this.users = []) });
  }

  private emptyForm() {
    return {
      departmentId: null, userId: null, reportingManagerId: null,
      firstName: '', lastName: '', email: '', phone: null, dateOfBirth: null, gender: null,
      jobTitle: '', employmentType: 'Full-Time', dateOfJoining: new Date()
    };
  }

  managersFor(employee: EmployeeDto | null): EmployeeDto[] {
    return this.employees.filter(e => !employee || e.id !== employee.id);
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.employees = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load employees.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(employee: EmployeeDto) {
    this.editing = employee;
    this.form = {
      departmentId: employee.departmentId, userId: employee.userId,
      reportingManagerId: employee.reportingManagerId,
      firstName: employee.firstName, lastName: employee.lastName, email: employee.email,
      phone: employee.phone, dateOfBirth: employee.dateOfBirth ? new Date(employee.dateOfBirth) : null,
      gender: employee.gender, jobTitle: employee.jobTitle, employmentType: employee.employmentType,
      dateOfJoining: new Date(employee.dateOfJoining)
    };
    this.showDialog = true;
  }

  save() {
    if ((!this.editing && !this.form.departmentId) || !this.form.firstName || !this.form.lastName
        || !this.form.email || !this.form.jobTitle) {
      this.notify.warn('Department, name, email, and job title are required.');
      return;
    }
    this.saving = true;
    const sharedFields = {
      userId: this.form.userId,
      reportingManagerId: this.form.reportingManagerId,
      firstName: this.form.firstName,
      lastName: this.form.lastName,
      email: this.form.email,
      phone: this.form.phone,
      dateOfBirth: this.form.dateOfBirth ? this.form.dateOfBirth.toISOString() : null,
      gender: this.form.gender,
      jobTitle: this.form.jobTitle,
      employmentType: this.form.employmentType,
      dateOfJoining: (this.form.dateOfJoining ?? new Date()).toISOString()
    };
    const req$ = this.editing
      ? this.api.update(this.editing.id, sharedFields as UpdateEmployeeDto)
      : this.api.create({ departmentId: this.form.departmentId!, ...sharedFields } as CreateEmployeeDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Employee ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save employee.');
      }
    });
  }

  delete(employee: EmployeeDto) {
    this.confirm.confirm({
      message: `Delete employee "${employee.firstName} ${employee.lastName}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(employee.id).subscribe({
          next: () => {
            this.notify.success('Employee deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete employee.')
        });
      }
    });
  }

  terminate(employee: EmployeeDto) {
    this.confirm.confirm({
      message: `Terminate "${employee.firstName} ${employee.lastName}"? This cannot be undone.`,
      header: 'Confirm Terminate',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.terminate(employee.id).subscribe({
          next: () => {
            this.notify.success('Employee terminated.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to terminate employee.')
        });
      }
    });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Active') return 'success';
    if (status === 'Terminated') return 'danger';
    if (status === 'OnLeave') return 'warn';
    return 'info';
  }
}

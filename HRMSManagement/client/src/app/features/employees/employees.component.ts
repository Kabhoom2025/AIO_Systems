import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { environment } from '../../../environments/environment';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import { EmployeesApiService, EmployeeListDto, CreateEmployeeDto } from '../../core/employees-api.service';

interface LookupOption {
  id: number;
  name: string;
}

@Component({
  selector: 'app-employees',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    DropdownModule,
    TagModule,
    ConfirmDialogModule,
    HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './employees.component.html',
  styleUrl: './employees.component.scss'
})
export class EmployeesComponent implements OnInit {
  employees: EmployeeListDto[] = [];
  totalRecords = 0;
  loading = false;
  rows = 20;

  search = '';
  selectedDepartmentId: number | null = null;
  selectedBranchId: number | null = null;
  selectedStatus: string | null = null;

  statusOptions = [
    { label: 'Active', value: 'Active' },
    { label: 'Inactive', value: 'Inactive' },
    { label: 'Exited', value: 'Exited' }
  ];

  branches: LookupOption[] = [];
  departments: LookupOption[] = [];
  designations: LookupOption[] = [];
  shifts: LookupOption[] = [];

  showAddDialog = false;
  saving = false;

  form: CreateEmployeeDto = this.emptyForm();

  private lastLazyEvent: TableLazyLoadEvent | null = null;

  constructor(
    private employeesApi: EmployeesApiService,
    private http: HttpClient,
    private router: Router,
    private notify: NotificationService,
    private confirmation: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.loadLookups();
  }

  loadLookups() {
    this.http.get<LookupOption[]>(`${environment.apiUrl}/branches`).subscribe({
      next: data => (this.branches = data),
      error: () => (this.branches = [])
    });
    this.http.get<LookupOption[]>(`${environment.apiUrl}/departments`).subscribe({
      next: data => (this.departments = data),
      error: () => (this.departments = [])
    });
    this.http.get<LookupOption[]>(`${environment.apiUrl}/designations`).subscribe({
      next: data => (this.designations = data),
      error: () => (this.designations = [])
    });
    this.http.get<LookupOption[]>(`${environment.apiUrl}/shifts`).subscribe({
      next: data => (this.shifts = data),
      error: () => (this.shifts = [])
    });
  }

  loadEmployees(event?: TableLazyLoadEvent) {
    if (event) this.lastLazyEvent = event;
    const first = this.lastLazyEvent?.first ?? 0;
    const rows = this.lastLazyEvent?.rows ?? this.rows;
    const page = Math.floor(first / rows) + 1;

    this.loading = true;
    this.employeesApi
      .getPaged(page, rows, this.search || null, this.selectedDepartmentId, this.selectedBranchId, this.selectedStatus)
      .subscribe({
        next: result => {
          this.employees = result.items;
          this.totalRecords = result.totalCount;
          this.loading = false;
        },
        error: err => {
          this.loading = false;
          this.notify.error(err.error?.message ?? 'Failed to load employees.');
        }
      });
  }

  onFilterChange() {
    this.loadEmployees({ first: 0, rows: this.rows });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'warn' | 'secondary' {
    switch (status) {
      case 'Active':
        return 'success';
      case 'Exited':
        return 'danger';
      case 'Inactive':
        return 'warn';
      default:
        return 'secondary';
    }
  }

  view(emp: EmployeeListDto) {
    this.router.navigate(['/employees', emp.id]);
  }

  openAddDialog() {
    this.form = this.emptyForm();
    this.showAddDialog = true;
  }

  save() {
    this.saving = true;
    this.employeesApi.create(this.form).subscribe({
      next: () => {
        this.saving = false;
        this.showAddDialog = false;
        this.notify.success('Employee created successfully.');
        this.loadEmployees();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to create employee.');
      }
    });
  }

  confirmDelete(emp: EmployeeListDto) {
    this.confirmation.confirm({
      message: `Delete employee "${emp.fullName}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.delete(emp)
    });
  }

  delete(emp: EmployeeListDto) {
    this.employeesApi.delete(emp.id).subscribe({
      next: () => {
        this.notify.success('Employee deleted.');
        this.loadEmployees();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to delete employee.')
    });
  }

  exportCsv() {
    this.employeesApi.exportCsv().subscribe({
      next: blob => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'employees.csv';
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to export employees.')
    });
  }

  private emptyForm(): CreateEmployeeDto {
    return {
      branchId: 0,
      departmentId: 0,
      designationId: 0,
      shiftId: null,
      managerId: null,
      firstName: '',
      lastName: '',
      gender: 'Male',
      workEmail: '',
      phone: '',
      joiningDate: new Date().toISOString().substring(0, 10),
      employmentType: 'FullTime'
    };
  }
}

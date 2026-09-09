import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  EmployeeCompensationApiService, EmployeeCompensationDto, CreateEmployeeCompensationDto, UpdateEmployeeCompensationDto
} from '../../../core/employee-compensation-api.service';
import { EmployeeApiService, EmployeeDto } from '../../../core/employee-api.service';

@Component({
  selector: 'app-employee-compensation',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputNumberModule, DropdownModule, ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './employee-compensation.component.html',
  styleUrl: './employee-compensation.component.scss'
})
export class EmployeeCompensationComponent implements OnInit {
  rows: EmployeeCompensationDto[] = [];
  employees: EmployeeDto[] = [];
  loading = false;

  showDialog = false;
  editing: EmployeeCompensationDto | null = null;
  saving = false;

  form: {
    employeeId: number | null;
    basicSalary: number | null;
    hra: number | null;
    otherAllowances: number | null;
    deductions: number | null;
  } = this.emptyForm();

  constructor(
    private api: EmployeeCompensationApiService,
    private employeeApi: EmployeeApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.employeeApi.getAll().subscribe({ next: rows => (this.employees = rows), error: () => (this.employees = []) });
  }

  private emptyForm() {
    return { employeeId: null, basicSalary: null, hra: null, otherAllowances: null, deductions: null };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.rows = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load employee compensation.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(row: EmployeeCompensationDto) {
    this.editing = row;
    this.form = {
      employeeId: row.employeeId, basicSalary: row.basicSalary,
      hra: row.hra, otherAllowances: row.otherAllowances, deductions: row.deductions
    };
    this.showDialog = true;
  }

  save() {
    if ((!this.editing && !this.form.employeeId) || this.form.basicSalary == null) {
      this.notify.warn('Employee and basic salary are required.');
      return;
    }
    this.saving = true;
    const sharedFields = {
      basicSalary: this.form.basicSalary!,
      hra: this.form.hra ?? 0,
      otherAllowances: this.form.otherAllowances ?? 0,
      deductions: this.form.deductions ?? 0
    };
    const req$ = this.editing
      ? this.api.update(this.editing.id, sharedFields as UpdateEmployeeCompensationDto)
      : this.api.create({ employeeId: this.form.employeeId!, ...sharedFields } as CreateEmployeeCompensationDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Compensation ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save compensation.');
      }
    });
  }

  delete(row: EmployeeCompensationDto) {
    this.confirm.confirm({
      message: `Delete compensation record for "${row.employeeName}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(row.id).subscribe({
          next: () => {
            this.notify.success('Compensation record deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete compensation record.')
        });
      }
    });
  }
}

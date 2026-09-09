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
  LeaveRequestApiService, LeaveRequestDto, CreateLeaveRequestDto, UpdateLeaveRequestDto
} from '../../../core/leave-request-api.service';
import { EmployeeApiService, EmployeeDto } from '../../../core/employee-api.service';
import { LeaveTypeApiService, LeaveTypeDto } from '../../../core/leave-type-api.service';

@Component({
  selector: 'app-leave-requests',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, DropdownModule, CalendarModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './leave-requests.component.html',
  styleUrl: './leave-requests.component.scss'
})
export class LeaveRequestsComponent implements OnInit {
  leaveRequests: LeaveRequestDto[] = [];
  employees: EmployeeDto[] = [];
  leaveTypes: LeaveTypeDto[] = [];
  loading = false;

  showDialog = false;
  editing: LeaveRequestDto | null = null;
  saving = false;

  form: {
    employeeId: number | null;
    leaveTypeId: number | null;
    startDate: Date | null;
    endDate: Date | null;
    daysRequested: number | null;
    reason: string | null;
  } = this.emptyForm();

  constructor(
    private api: LeaveRequestApiService,
    private employeeApi: EmployeeApiService,
    private leaveTypeApi: LeaveTypeApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.employeeApi.getAll().subscribe({ next: rows => (this.employees = rows), error: () => (this.employees = []) });
    this.leaveTypeApi.getAll().subscribe({ next: rows => (this.leaveTypes = rows), error: () => (this.leaveTypes = []) });
  }

  private emptyForm() {
    return {
      employeeId: null, leaveTypeId: null,
      startDate: new Date(), endDate: new Date(), daysRequested: null, reason: null
    };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.leaveRequests = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load leave requests.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(request: LeaveRequestDto) {
    this.editing = request;
    this.form = {
      employeeId: request.employeeId, leaveTypeId: request.leaveTypeId,
      startDate: new Date(request.startDate), endDate: new Date(request.endDate),
      daysRequested: request.daysRequested, reason: request.reason
    };
    this.showDialog = true;
  }

  save() {
    if ((!this.editing && !this.form.employeeId) || !this.form.leaveTypeId || !this.form.daysRequested) {
      this.notify.warn('Employee, leave type, and days requested are required.');
      return;
    }
    this.saving = true;
    const sharedFields = {
      leaveTypeId: this.form.leaveTypeId!,
      startDate: (this.form.startDate ?? new Date()).toISOString(),
      endDate: (this.form.endDate ?? new Date()).toISOString(),
      daysRequested: this.form.daysRequested!,
      reason: this.form.reason
    };
    const req$ = this.editing
      ? this.api.update(this.editing.id, sharedFields as UpdateLeaveRequestDto)
      : this.api.create({ employeeId: this.form.employeeId!, ...sharedFields } as CreateLeaveRequestDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Leave request ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save leave request.');
      }
    });
  }

  delete(request: LeaveRequestDto) {
    this.confirm.confirm({
      message: `Delete this leave request for "${request.employeeName}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(request.id).subscribe({
          next: () => {
            this.notify.success('Leave request deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete leave request.')
        });
      }
    });
  }

  approve(request: LeaveRequestDto) {
    this.api.approve(request.id).subscribe({
      next: () => {
        this.notify.success('Leave request approved.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to approve leave request.')
    });
  }

  reject(request: LeaveRequestDto) {
    this.api.reject(request.id).subscribe({
      next: () => {
        this.notify.success('Leave request rejected.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to reject leave request.')
    });
  }

  cancel(request: LeaveRequestDto) {
    this.confirm.confirm({
      message: `Cancel this leave request for "${request.employeeName}"?`,
      header: 'Confirm Cancel',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.cancel(request.id).subscribe({
          next: () => {
            this.notify.success('Leave request cancelled.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to cancel leave request.')
        });
      }
    });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Approved') return 'success';
    if (status === 'Rejected' || status === 'Cancelled') return 'danger';
    return 'info';
  }
}

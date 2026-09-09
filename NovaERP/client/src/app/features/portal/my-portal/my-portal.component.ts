import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TabViewModule } from 'primeng/tabview';
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
import { NotificationService } from '../../../core/notification.service';
import { EmployeeDto } from '../../../core/employee-api.service';
import { LeaveRequestDto } from '../../../core/leave-request-api.service';
import { LeaveTypeDto } from '../../../core/leave-type-api.service';
import {
  EmployeePortalApiService, CreateMyLeaveRequestDto, MyPayslipDto
} from '../../../core/employee-portal-api.service';

@Component({
  selector: 'app-my-portal',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TabViewModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, DropdownModule, CalendarModule, TagModule,
    ToastModule, ConfirmDialogModule
  ],
  providers: [ConfirmationService],
  templateUrl: './my-portal.component.html',
  styleUrl: './my-portal.component.scss'
})
export class MyPortalComponent implements OnInit {
  loading = false;
  noEmployeeLinked = false;

  profile: EmployeeDto | null = null;
  leaveRequests: LeaveRequestDto[] = [];
  payslips: MyPayslipDto[] = [];
  leaveTypes: LeaveTypeDto[] = [];

  months = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];

  showDialog = false;
  saving = false;
  form: {
    leaveTypeId: number | null;
    startDate: Date | null;
    endDate: Date | null;
    daysRequested: number | null;
    reason: string | null;
  } = this.emptyForm();

  constructor(
    private api: EmployeePortalApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  private emptyForm() {
    return { leaveTypeId: null, startDate: new Date(), endDate: new Date(), daysRequested: null, reason: null };
  }

  monthName(month: number): string {
    return this.months[month - 1] ?? String(month);
  }

  load() {
    this.loading = true;
    this.noEmployeeLinked = false;
    this.api.getMyProfile().subscribe({
      next: profile => {
        this.profile = profile;
        this.loading = false;
        this.loadLeaveRequests();
        this.loadPayslips();
        this.api.getLeaveTypes().subscribe({ next: rows => (this.leaveTypes = rows), error: () => (this.leaveTypes = []) });
      },
      error: err => {
        this.loading = false;
        if (err.status === 404) {
          this.noEmployeeLinked = true;
        } else {
          this.notify.error(err.error?.message ?? 'Failed to load your profile.');
        }
      }
    });
  }

  private loadLeaveRequests() {
    this.api.getMyLeaveRequests().subscribe({
      next: rows => (this.leaveRequests = rows),
      error: err => this.notify.error(err.error?.message ?? 'Failed to load your leave requests.')
    });
  }

  private loadPayslips() {
    this.api.getMyPayslips().subscribe({
      next: rows => (this.payslips = rows),
      error: err => this.notify.error(err.error?.message ?? 'Failed to load your payslips.')
    });
  }

  openNew() {
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  save() {
    if (!this.form.leaveTypeId || this.form.daysRequested == null) {
      this.notify.warn('Leave type and days requested are required.');
      return;
    }
    this.saving = true;
    const dto: CreateMyLeaveRequestDto = {
      leaveTypeId: this.form.leaveTypeId,
      startDate: (this.form.startDate ?? new Date()).toISOString(),
      endDate: (this.form.endDate ?? new Date()).toISOString(),
      daysRequested: this.form.daysRequested,
      reason: this.form.reason
    };
    this.api.createMyLeaveRequest(dto).subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success('Leave request filed.');
        this.loadLeaveRequests();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to file leave request.');
      }
    });
  }

  cancel(request: LeaveRequestDto) {
    this.confirm.confirm({
      message: 'Cancel this leave request?',
      header: 'Confirm Cancel',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.cancelMyLeaveRequest(request.id).subscribe({
          next: () => {
            this.notify.success('Leave request cancelled.');
            this.loadLeaveRequests();
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

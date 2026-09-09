import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { TabViewModule } from 'primeng/tabview';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { CalendarModule } from 'primeng/calendar';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import { getEmployeeIdFromToken } from '../../core/auth.helper';
import {
  AttendanceApiService,
  AttendanceRecordDto,
  AttendanceDayDto,
  AttendanceSummaryDto,
  RegularizationDto
} from '../../core/attendance-api.service';

@Component({
  selector: 'app-attendance',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TabViewModule,
    TableModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    InputTextarea,
    DropdownModule,
    TagModule,
    CalendarModule,
    ToastModule,
    ConfirmDialogModule,
    HasPermissionDirective
  ],
  templateUrl: './attendance.component.html',
  styleUrl: './attendance.component.scss'
})
export class AttendanceComponent implements OnInit {
  employeeId = getEmployeeIdFromToken();

  // My Attendance tab
  todayRecord: AttendanceRecordDto | null = null;
  todayLoading = false;
  checkingIn = false;
  checkingOut = false;

  years: number[] = [];
  months = [
    { label: 'January', value: 1 }, { label: 'February', value: 2 }, { label: 'March', value: 3 },
    { label: 'April', value: 4 }, { label: 'May', value: 5 }, { label: 'June', value: 6 },
    { label: 'July', value: 7 }, { label: 'August', value: 8 }, { label: 'September', value: 9 },
    { label: 'October', value: 10 }, { label: 'November', value: 11 }, { label: 'December', value: 12 }
  ];
  selectedYear = new Date().getFullYear();
  selectedMonth = new Date().getMonth() + 1;
  myRecords: AttendanceRecordDto[] = [];
  myRecordsLoading = false;

  // Daily view tab
  dailyDate: Date = new Date();
  dailyRecords: AttendanceDayDto[] = [];
  dailyLoading = false;
  summary: AttendanceSummaryDto | null = null;

  // Regularizations tab
  myRegularizations: RegularizationDto[] = [];
  myRegularizationsLoading = false;
  approveQueue: RegularizationDto[] = [];
  approveQueueLoading = false;

  showNewRequestDialog = false;
  newRequest = {
    date: new Date(),
    requestedCheckIn: null as Date | null,
    requestedCheckOut: null as Date | null,
    reason: ''
  };
  savingRequest = false;

  showReviewDialog = false;
  reviewAction: 'approve' | 'reject' | null = null;
  reviewTarget: RegularizationDto | null = null;
  reviewNotes = '';
  reviewSaving = false;

  constructor(private api: AttendanceApiService, private notify: NotificationService) {
    const currentYear = new Date().getFullYear();
    for (let y = currentYear - 2; y <= currentYear; y++) this.years.push(y);
  }

  ngOnInit() {
    this.loadToday();
    this.loadMyRecords();
    this.loadDaily();
    this.loadSummary();
    this.loadMyRegularizations();
    this.loadApproveQueue();
  }

  // ---------- My Attendance ----------
  loadToday() {
    this.todayLoading = true;
    this.api.today().subscribe({
      next: rec => { this.todayRecord = rec; this.todayLoading = false; },
      error: err => {
        this.todayLoading = false;
        this.todayRecord = null;
        this.notify.error(err.error?.message ?? 'Failed to load today\'s attendance.');
      }
    });
  }

  checkIn() {
    this.checkingIn = true;
    this.api.checkIn({ source: 'Web' }).subscribe({
      next: rec => {
        this.checkingIn = false;
        this.todayRecord = rec;
        this.notify.success('Checked in successfully.');
        this.loadMyRecords();
      },
      error: err => {
        this.checkingIn = false;
        this.notify.error(err.error?.message ?? 'Check-in failed.');
      }
    });
  }

  checkOut() {
    this.checkingOut = true;
    this.api.checkOut().subscribe({
      next: rec => {
        this.checkingOut = false;
        this.todayRecord = rec;
        this.notify.success('Checked out successfully.');
        this.loadMyRecords();
      },
      error: err => {
        this.checkingOut = false;
        this.notify.error(err.error?.message ?? 'Check-out failed.');
      }
    });
  }

  loadMyRecords() {
    this.myRecordsLoading = true;
    this.api.my(this.selectedYear, this.selectedMonth).subscribe({
      next: recs => { this.myRecords = recs; this.myRecordsLoading = false; },
      error: err => {
        this.myRecordsLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load attendance records.');
      }
    });
  }

  onPeriodChange() {
    this.loadMyRecords();
  }

  // ---------- Daily View ----------
  private toDateOnly(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  loadDaily() {
    this.dailyLoading = true;
    this.api.daily(this.toDateOnly(this.dailyDate)).subscribe({
      next: recs => { this.dailyRecords = recs; this.dailyLoading = false; },
      error: err => {
        this.dailyLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load daily attendance.');
      }
    });
  }

  loadSummary() {
    this.api.summary(this.toDateOnly(this.dailyDate)).subscribe({
      next: s => { this.summary = s; },
      error: err => this.notify.error(err.error?.message ?? 'Failed to load summary.')
    });
  }

  onDailyDateChange() {
    this.loadDaily();
    this.loadSummary();
  }

  // ---------- Regularizations ----------
  loadMyRegularizations() {
    this.myRegularizationsLoading = true;
    this.api.myRegularizations().subscribe({
      next: recs => { this.myRegularizations = recs; this.myRegularizationsLoading = false; },
      error: err => {
        this.myRegularizationsLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load regularization requests.');
      }
    });
  }

  loadApproveQueue() {
    this.approveQueueLoading = true;
    this.api.regularizations().subscribe({
      next: recs => {
        this.approveQueue = recs.filter(r => r.status === 'Pending');
        this.approveQueueLoading = false;
      },
      error: () => { this.approveQueueLoading = false; }
    });
  }

  openNewRequest() {
    this.newRequest = { date: new Date(), requestedCheckIn: null, requestedCheckOut: null, reason: '' };
    this.showNewRequestDialog = true;
  }

  submitNewRequest() {
    if (!this.newRequest.reason.trim()) {
      this.notify.warn('Please provide a reason.');
      return;
    }
    this.savingRequest = true;
    this.api.createRegularization({
      date: this.toDateOnly(this.newRequest.date),
      requestedCheckIn: this.newRequest.requestedCheckIn ? this.newRequest.requestedCheckIn.toISOString() : null,
      requestedCheckOut: this.newRequest.requestedCheckOut ? this.newRequest.requestedCheckOut.toISOString() : null,
      reason: this.newRequest.reason
    }).subscribe({
      next: () => {
        this.savingRequest = false;
        this.showNewRequestDialog = false;
        this.notify.success('Regularization request submitted.');
        this.loadMyRegularizations();
        this.loadApproveQueue();
      },
      error: err => {
        this.savingRequest = false;
        this.notify.error(err.error?.message ?? 'Failed to submit request.');
      }
    });
  }

  openReview(target: RegularizationDto, action: 'approve' | 'reject') {
    this.reviewTarget = target;
    this.reviewAction = action;
    this.reviewNotes = '';
    this.showReviewDialog = true;
  }

  submitReview() {
    if (!this.reviewTarget || !this.reviewAction) return;
    this.reviewSaving = true;
    const dto = { notes: this.reviewNotes };
    const req$ = this.reviewAction === 'approve'
      ? this.api.approveRegularization(this.reviewTarget.id, dto)
      : this.api.rejectRegularization(this.reviewTarget.id, dto);
    req$.subscribe({
      next: () => {
        this.reviewSaving = false;
        this.showReviewDialog = false;
        this.notify.success(`Request ${this.reviewAction === 'approve' ? 'approved' : 'rejected'}.`);
        this.loadApproveQueue();
        this.loadMyRegularizations();
      },
      error: err => {
        this.reviewSaving = false;
        this.notify.error(err.error?.message ?? 'Failed to submit review.');
      }
    });
  }

  statusSeverity(status: string): 'success' | 'warn' | 'danger' | 'secondary' | 'info' {
    switch (status) {
      case 'Approved': return 'success';
      case 'Pending': return 'warn';
      case 'Rejected': return 'danger';
      case 'Present': return 'success';
      case 'Absent': return 'danger';
      case 'Late': return 'warn';
      case 'OnLeave': return 'info';
      default: return 'secondary';
    }
  }
}

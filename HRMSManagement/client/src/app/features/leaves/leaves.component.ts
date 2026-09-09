import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { TabViewModule } from 'primeng/tabview';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { CalendarModule } from 'primeng/calendar';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';

import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import {
  LeaveApiService,
  LeaveTypeDto,
  LeaveBalanceDto,
  LeaveRequestDto,
  LeaveCalendarDto
} from '../../core/leave-api.service';

@Component({
  selector: 'app-leaves',
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
    InputNumberModule,
    DropdownModule,
    TagModule,
    CalendarModule,
    CheckboxModule,
    ToastModule,
    ConfirmDialogModule,
    HasPermissionDirective
  ],
  templateUrl: './leaves.component.html',
  styleUrl: './leaves.component.scss'
})
export class LeavesComponent implements OnInit {
  // ------- shared -------
  leaveTypes: LeaveTypeDto[] = [];

  // ------- My Leaves tab -------
  balances: LeaveBalanceDto[] = [];
  balancesLoading = false;
  myRequests: LeaveRequestDto[] = [];
  myRequestsLoading = false;

  showApplyDialog = false;
  applyForm = {
    leaveTypeId: null as number | null,
    startDate: new Date(),
    endDate: new Date(),
    isHalfDay: false,
    reason: ''
  };
  applySaving = false;

  // ------- Approvals tab -------
  pendingRequests: LeaveRequestDto[] = [];
  pendingLoading = false;

  showReviewDialog = false;
  reviewAction: 'approve' | 'reject' | null = null;
  reviewTarget: LeaveRequestDto | null = null;
  reviewNotes = '';
  reviewSaving = false;

  // ------- Leave Types tab -------
  typesLoading = false;
  showTypeDialog = false;
  editingTypeId: number | null = null;
  typeForm = {
    name: '',
    code: '',
    isPaid: true,
    annualQuota: 0,
    maxCarryForward: 0,
    requiresApproval: true,
    color: '#4f46e5',
    isActive: true
  };
  typeSaving = false;

  // ------- Calendar tab -------
  calendarEntries: LeaveCalendarDto[] = [];
  calendarLoading = false;

  constructor(
    private api: LeaveApiService,
    private notify: NotificationService,
    private confirmation: ConfirmationService
  ) {}

  ngOnInit() {
    this.loadTypes();
    this.loadBalances();
    this.loadMyRequests();
    this.loadPendingRequests();
    this.loadCalendar();
  }

  private toDateOnly(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  // ---------- shared ----------
  loadTypes() {
    this.typesLoading = true;
    this.api.getTypes().subscribe({
      next: types => { this.leaveTypes = types; this.typesLoading = false; },
      error: err => {
        this.typesLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load leave types.');
      }
    });
  }

  // ---------- My Leaves ----------
  loadBalances() {
    this.balancesLoading = true;
    this.api.myBalances().subscribe({
      next: b => { this.balances = b; this.balancesLoading = false; },
      error: err => {
        this.balancesLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load leave balances.');
      }
    });
  }

  loadMyRequests() {
    this.myRequestsLoading = true;
    this.api.myRequests().subscribe({
      next: r => { this.myRequests = r; this.myRequestsLoading = false; },
      error: err => {
        this.myRequestsLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load leave requests.');
      }
    });
  }

  openApplyDialog() {
    this.applyForm = {
      leaveTypeId: this.leaveTypes.length ? this.leaveTypes[0].id : null,
      startDate: new Date(),
      endDate: new Date(),
      isHalfDay: false,
      reason: ''
    };
    this.showApplyDialog = true;
  }

  submitApply() {
    if (!this.applyForm.leaveTypeId) {
      this.notify.warn('Please select a leave type.');
      return;
    }
    if (!this.applyForm.reason.trim()) {
      this.notify.warn('Please provide a reason.');
      return;
    }
    this.applySaving = true;
    this.api.createRequest({
      leaveTypeId: this.applyForm.leaveTypeId,
      startDate: this.toDateOnly(this.applyForm.startDate),
      endDate: this.toDateOnly(this.applyForm.endDate),
      isHalfDay: this.applyForm.isHalfDay,
      reason: this.applyForm.reason
    }).subscribe({
      next: () => {
        this.applySaving = false;
        this.showApplyDialog = false;
        this.notify.success('Leave request submitted.');
        this.loadMyRequests();
        this.loadBalances();
        this.loadPendingRequests();
      },
      error: err => {
        this.applySaving = false;
        this.notify.error(err.error?.message ?? 'Failed to submit leave request.');
      }
    });
  }

  cancelRequest(req: LeaveRequestDto) {
    this.confirmation.confirm({
      message: `Cancel leave request from ${req.startDate} to ${req.endDate}?`,
      header: 'Confirm Cancellation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.cancelRequest(req.id).subscribe({
          next: () => {
            this.notify.success('Leave request cancelled.');
            this.loadMyRequests();
            this.loadBalances();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to cancel request.')
        });
      }
    });
  }

  canCancel(req: LeaveRequestDto): boolean {
    return req.status === 'Pending' || req.status === 'Approved';
  }

  // ---------- Approvals ----------
  loadPendingRequests() {
    this.pendingLoading = true;
    this.api.pendingRequests().subscribe({
      next: r => { this.pendingRequests = r; this.pendingLoading = false; },
      error: () => { this.pendingLoading = false; }
    });
  }

  openReview(target: LeaveRequestDto, action: 'approve' | 'reject') {
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
      ? this.api.approveRequest(this.reviewTarget.id, dto)
      : this.api.rejectRequest(this.reviewTarget.id, dto);
    req$.subscribe({
      next: () => {
        this.reviewSaving = false;
        this.showReviewDialog = false;
        this.notify.success(`Leave request ${this.reviewAction === 'approve' ? 'approved' : 'rejected'}.`);
        this.loadPendingRequests();
      },
      error: err => {
        this.reviewSaving = false;
        this.notify.error(err.error?.message ?? 'Failed to submit review.');
      }
    });
  }

  // ---------- Leave Types ----------
  openAddType() {
    this.editingTypeId = null;
    this.typeForm = {
      name: '', code: '', isPaid: true, annualQuota: 0, maxCarryForward: 0,
      requiresApproval: true, color: '#4f46e5', isActive: true
    };
    this.showTypeDialog = true;
  }

  openEditType(t: LeaveTypeDto) {
    this.editingTypeId = t.id;
    this.typeForm = {
      name: t.name,
      code: t.code,
      isPaid: t.isPaid,
      annualQuota: t.annualQuota,
      maxCarryForward: t.maxCarryForward,
      requiresApproval: t.requiresApproval,
      color: t.color ?? '#4f46e5',
      isActive: t.isActive
    };
    this.showTypeDialog = true;
  }

  saveType() {
    if (!this.typeForm.name.trim() || !this.typeForm.code.trim()) {
      this.notify.warn('Name and code are required.');
      return;
    }
    this.typeSaving = true;
    const basePayload = {
      name: this.typeForm.name,
      code: this.typeForm.code,
      isPaid: this.typeForm.isPaid,
      annualQuota: this.typeForm.annualQuota,
      maxCarryForward: this.typeForm.maxCarryForward,
      requiresApproval: this.typeForm.requiresApproval,
      color: this.typeForm.color
    };
    const req$ = this.editingTypeId
      ? this.api.updateType(this.editingTypeId, { ...basePayload, isActive: this.typeForm.isActive })
      : this.api.createType(basePayload);
    req$.subscribe({
      next: () => {
        this.typeSaving = false;
        this.showTypeDialog = false;
        this.notify.success(`Leave type ${this.editingTypeId ? 'updated' : 'created'}.`);
        this.loadTypes();
      },
      error: err => {
        this.typeSaving = false;
        this.notify.error(err.error?.message ?? 'Failed to save leave type.');
      }
    });
  }

  deleteType(t: LeaveTypeDto) {
    this.confirmation.confirm({
      message: `Delete leave type "${t.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.deleteType(t.id).subscribe({
          next: () => {
            this.notify.success('Leave type deleted.');
            this.loadTypes();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete leave type.')
        });
      }
    });
  }

  // ---------- Calendar ----------
  loadCalendar() {
    const now = new Date();
    const from = new Date(now.getFullYear(), now.getMonth(), 1);
    const to = new Date(now.getFullYear(), now.getMonth() + 1, 0);
    this.calendarLoading = true;
    this.api.calendar(this.toDateOnly(from), this.toDateOnly(to)).subscribe({
      next: entries => { this.calendarEntries = entries; this.calendarLoading = false; },
      error: () => { this.calendarLoading = false; }
    });
  }

  statusSeverity(status: string): 'success' | 'warn' | 'danger' | 'secondary' {
    switch (status) {
      case 'Approved': return 'success';
      case 'Pending': return 'warn';
      case 'Rejected': return 'danger';
      case 'Cancelled': return 'secondary';
      default: return 'secondary';
    }
  }
}

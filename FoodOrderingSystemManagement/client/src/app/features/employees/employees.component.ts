import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { MatIconModule }            from '@angular/material/icon';
import { MatButtonModule }          from '@angular/material/button';
import { MatFormFieldModule }       from '@angular/material/form-field';
import { MatInputModule }           from '@angular/material/input';
import { MatSelectModule }          from '@angular/material/select';
import { MatTooltipModule }         from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog }                from '@angular/material/dialog';
import { MatTabsModule }            from '@angular/material/tabs';
import { MatChipsModule }           from '@angular/material/chips';

import { UserService }     from '../../core/services/user.service';
import { RoleService }     from '../../core/services/role.service';
import { EmployeeService } from '../../core/services/employee.service';
import { NotificationService } from '../../shared/services/notification.service';
import { ConfirmationDialogComponent } from '../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { HasPermissionDirective }  from '../../core/directives/has-permission.directive';
import { AuthService }             from '../../core/authentication/auth.service';
import { APP_CONSTANTS }           from '../../core/constants/app.constants';
import { AppUser }                 from '../../core/models/user.model';
import { Role }                    from '../../core/models/role.model';
import {
  AttendanceRecord, UpsertAttendanceRequest,
  EmployeeSalaryConfig, UpdateSalaryConfigRequest,
  SalaryPayment, CreateSalaryPaymentRequest,
  ShiftDefinition, CreateShiftDefinitionRequest,
  ShiftAssignment, CreateShiftAssignmentRequest,
  PerformanceReview, CreatePerformanceReviewRequest,
  UpdateUserRequest,
} from '../../core/models/employee.model';

@Component({
  selector: 'app-employees',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatIconModule, MatButtonModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatTooltipModule,
    MatProgressSpinnerModule, MatTabsModule, MatChipsModule,
    HasPermissionDirective,
  ],
  templateUrl: './employees.component.html',
  styleUrl:    './employees.component.scss',
})
export class EmployeesComponent implements OnInit {
  private userSvc   = inject(UserService);
  private roleSvc   = inject(RoleService);
  private empSvc    = inject(EmployeeService);
  private auth      = inject(AuthService);
  private notify    = inject(NotificationService);
  private dialog    = inject(MatDialog);
  private fb        = inject(FormBuilder);

  get isAdmin(): boolean { return this.auth.userRole() === APP_CONSTANTS.ROLES.ADMIN; }
  get currentUserId(): number { return this.auth.currentUser()?.userId ?? 0; }

  loading = signal(true);
  users   = signal<AppUser[]>([]);
  roles   = signal<Role[]>([]);

  // ── Edit user ──────────────────────────────────────────────────────────
  editingUser  = signal<AppUser | null>(null);
  saving       = signal(false);
  deleting     = signal<number | null>(null);

  editForm = this.fb.group({
    name:   ['', [Validators.required, Validators.maxLength(100)]],
    email:  ['', [Validators.required, Validators.email]],
    roleId: [0,  Validators.required],
  });

  // ── Attendance ─────────────────────────────────────────────────────────
  attendanceDate   = signal(new Date().toISOString().substring(0, 10));
  attendance       = signal<AttendanceRecord[]>([]);
  attendanceLoading = signal(false);
  pendingAttendance = signal<Map<number, UpsertAttendanceRequest>>(new Map());
  savingAttendance = signal(false);

  readonly statuses = ['Present', 'Absent', 'Late', 'HalfDay', 'Leave'];

  // ── Salary ─────────────────────────────────────────────────────────────
  salaryConfigs   = signal<EmployeeSalaryConfig[]>([]);
  salaryLoading   = signal(false);
  editingSalary   = signal<EmployeeSalaryConfig | null>(null);
  salaryForm = this.fb.group({
    basicSalary: [0, [Validators.required, Validators.min(0)]],
    allowances:  [0, [Validators.required, Validators.min(0)]],
    payPeriod:   ['Monthly', Validators.required],
  });
  payments       = signal<SalaryPayment[]>([]);
  addingPayment  = signal(false);
  paymentForm = this.fb.group({
    userId:     [0,  Validators.required],
    month:      [new Date().getMonth() + 1, [Validators.required, Validators.min(1), Validators.max(12)]],
    year:       [new Date().getFullYear(), [Validators.required, Validators.min(2020)]],
    grossPay:   [0,  [Validators.required, Validators.min(0)]],
    deductions: [0,  [Validators.required, Validators.min(0)]],
    notes:      [''],
  });
  markingPaid = signal<number | null>(null);

  // ── Shifts ─────────────────────────────────────────────────────────────
  shiftDefs    = signal<ShiftDefinition[]>([]);
  shiftsLoading = signal(false);
  addingShift  = signal(false);
  shiftForm = this.fb.group({
    name:      ['', Validators.required],
    startTime: ['09:00', Validators.required],
    endTime:   ['17:00', Validators.required],
    colorCode: ['#1976d2'],
  });
  shiftWeekStart  = signal(this.getMonday(new Date()));
  shiftAssignments = signal<ShiftAssignment[]>([]);
  newAssignment    = signal<CreateShiftAssignmentRequest>({ userId: 0, shiftId: 0, date: '' });

  shiftWeekDays = computed(() => {
    const days: Date[] = [];
    const start = this.shiftWeekStart();
    for (let i = 0; i < 7; i++) {
      const d = new Date(start);
      d.setDate(start.getDate() + i);
      days.push(d);
    }
    return days;
  });

  // ── Performance ────────────────────────────────────────────────────────
  reviews        = signal<PerformanceReview[]>([]);
  reviewsLoading = signal(false);
  reviewFilter   = signal(0);
  addingReview   = signal(false);
  reviewForm = this.fb.group({
    userId:     [0,  Validators.required],
    reviewDate: [new Date().toISOString().substring(0, 10), Validators.required],
    rating:     [3,  [Validators.required, Validators.min(1), Validators.max(5)]],
    category:   ['General', Validators.required],
    comments:   ['', Validators.required],
  });

  readonly reviewCategories = ['General', 'Monthly', 'Quarterly', 'Annual'];
  readonly ratingStars = [1, 2, 3, 4, 5];
  readonly monthNames = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];

  filteredReviews = computed(() => {
    const uid = this.reviewFilter();
    return uid ? this.reviews().filter(r => r.userId === uid) : this.reviews();
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    forkJoin({ users: this.userSvc.getAll(), roles: this.roleSvc.getAll() }).subscribe({
      next: ({ users, roles }) => {
        this.users.set(users.data ?? []);
        this.roles.set(roles.data ?? []);
        this.loading.set(false);
        this.loadAttendance();
        this.loadSalary();
        this.loadShifts();
        this.loadReviews();
      },
      error: () => { this.notify.error('Failed to load employees.'); this.loading.set(false); },
    });
  }

  // ── Tab change ─────────────────────────────────────────────────────────
  onTabChange(idx: number): void {
    if (idx === 1) this.loadAttendance();
    if (idx === 2) this.loadSalary();
    if (idx === 3) this.loadShifts();
    if (idx === 4) this.loadReviews();
  }

  // ── Edit User ──────────────────────────────────────────────────────────

  startEdit(user: AppUser): void {
    this.editingUser.set(user);
    this.editForm.patchValue({ name: user.name, email: user.email, roleId: user.roleId });
  }

  cancelEdit(): void { this.editingUser.set(null); }

  saveEdit(): void {
    if (this.editForm.invalid) { this.editForm.markAllAsTouched(); return; }
    const user = this.editingUser();
    if (!user) return;
    this.saving.set(true);
    const dto: UpdateUserRequest = {
      name:   this.editForm.value.name!,
      email:  this.editForm.value.email!,
      roleId: this.editForm.value.roleId!,
    };
    this.empSvc.updateUser(user.id, dto).subscribe({
      next: () => {
        this.saving.set(false);
        this.editingUser.set(null);
        this.notify.success('Employee updated.');
        this.load();
      },
      error: err => { this.saving.set(false); this.notify.error(err?.error?.message || 'Update failed.'); },
    });
  }

  deleteEmployee(user: AppUser): void {
    const ref = this.dialog.open(ConfirmationDialogComponent, {
      data: { title: 'Delete Employee', message: `Permanently delete "${user.name}"? Their login access will be removed.`, confirmText: 'Delete', danger: true },
      width: '420px',
    });
    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.deleting.set(user.id);
      this.empSvc.deleteUser(user.id).subscribe({
        next: () => { this.deleting.set(null); this.notify.success(`"${user.name}" deleted.`); this.load(); },
        error: err => { this.deleting.set(null); this.notify.error(err?.error?.message || 'Delete failed.'); },
      });
    });
  }

  // ── Attendance ─────────────────────────────────────────────────────────

  loadAttendance(): void {
    this.attendanceLoading.set(true);
    this.empSvc.getAttendanceByDate(this.attendanceDate()).subscribe({
      next: res => {
        const map = new Map(this.pendingAttendance());
        // pre-populate pending from existing records
        (res.data ?? []).forEach(a => {
          if (!map.has(a.userId)) {
            map.set(a.userId, {
              userId: a.userId, date: this.attendanceDate(),
              status: a.status, checkIn: a.checkIn, checkOut: a.checkOut, notes: a.notes,
            });
          }
        });
        this.pendingAttendance.set(map);
        this.attendance.set(res.data ?? []);
        this.attendanceLoading.set(false);
      },
      error: () => this.attendanceLoading.set(false),
    });
  }

  getAttendancePending(userId: number): UpsertAttendanceRequest {
    const map = this.pendingAttendance();
    if (!map.has(userId)) {
      map.set(userId, { userId, date: this.attendanceDate(), status: 'Present' });
    }
    return map.get(userId)!;
  }

  setAttendanceStatus(userId: number, status: string): void {
    const map = new Map(this.pendingAttendance());
    const rec = map.get(userId) ?? { userId, date: this.attendanceDate(), status };
    map.set(userId, { ...rec, status });
    this.pendingAttendance.set(map);
  }

  setAttendanceTime(userId: number, field: 'checkIn' | 'checkOut', val: string): void {
    const map = new Map(this.pendingAttendance());
    const rec = map.get(userId) ?? { userId, date: this.attendanceDate(), status: 'Present' };
    map.set(userId, { ...rec, [field]: val });
    this.pendingAttendance.set(map);
  }

  onAttendanceDateChange(val: string): void {
    this.attendanceDate.set(val);
    this.pendingAttendance.set(new Map());
    this.loadAttendance();
  }

  saveAllAttendance(): void {
    this.savingAttendance.set(true);
    const reqs = Array.from(this.pendingAttendance().values());
    let done = 0;
    reqs.forEach(req => {
      this.empSvc.upsertAttendance({ ...req, date: this.attendanceDate() }).subscribe({
        next: () => { done++; if (done === reqs.length) { this.savingAttendance.set(false); this.notify.success('Attendance saved.'); this.loadAttendance(); } },
        error: () => { this.savingAttendance.set(false); this.notify.error('Failed to save attendance.'); },
      });
    });
    if (!reqs.length) { this.savingAttendance.set(false); }
  }

  getExistingStatus(userId: number): string {
    return this.attendance().find(a => a.userId === userId)?.status ?? '';
  }

  statusColor(s: string): string {
    switch (s) {
      case 'Present':  return '#2e7d32';
      case 'Absent':   return '#c62828';
      case 'Late':     return '#e65100';
      case 'HalfDay':  return '#1565c0';
      case 'Leave':    return '#7b1fa2';
      default: return '#888';
    }
  }

  // ── Salary ─────────────────────────────────────────────────────────────

  loadSalary(): void {
    this.salaryLoading.set(true);
    forkJoin({
      configs:  this.empSvc.getSalaryConfigs(),
      payments: this.empSvc.getPayments(),
    }).subscribe({
      next: ({ configs, payments }) => {
        this.salaryConfigs.set(configs.data ?? []);
        this.payments.set(payments.data ?? []);
        this.salaryLoading.set(false);
      },
      error: () => this.salaryLoading.set(false),
    });
  }

  startEditSalary(cfg: EmployeeSalaryConfig): void {
    this.editingSalary.set(cfg);
    this.salaryForm.patchValue({ basicSalary: cfg.basicSalary, allowances: cfg.allowances, payPeriod: cfg.payPeriod });
  }

  cancelEditSalary(): void { this.editingSalary.set(null); }

  saveSalary(): void {
    const cfg = this.editingSalary();
    if (!cfg || this.salaryForm.invalid) return;
    const req: UpdateSalaryConfigRequest = this.salaryForm.value as UpdateSalaryConfigRequest;
    this.empSvc.updateSalaryConfig(cfg.userId, req).subscribe({
      next: () => { this.editingSalary.set(null); this.notify.success('Salary updated.'); this.loadSalary(); },
      error: err => this.notify.error(err?.error?.message || 'Failed.'),
    });
  }

  submitPayment(): void {
    if (this.paymentForm.invalid) { this.paymentForm.markAllAsTouched(); return; }
    const req: CreateSalaryPaymentRequest = this.paymentForm.value as CreateSalaryPaymentRequest;
    this.empSvc.createPayment(req).subscribe({
      next: () => { this.addingPayment.set(false); this.notify.success('Payment record created.'); this.paymentForm.reset(); this.loadSalary(); },
      error: err => this.notify.error(err?.error?.message || 'Failed.'),
    });
  }

  markPaymentPaid(id: number): void {
    this.markingPaid.set(id);
    this.empSvc.markPaid(id).subscribe({
      next: () => { this.markingPaid.set(null); this.notify.success('Payment marked as paid.'); this.loadSalary(); },
      error: () => { this.markingPaid.set(null); this.notify.error('Failed.'); },
    });
  }

  // ── Shifts ─────────────────────────────────────────────────────────────

  loadShifts(): void {
    this.shiftsLoading.set(true);
    const start = this.shiftWeekStart();
    const end   = new Date(start); end.setDate(start.getDate() + 6);
    forkJoin({
      defs:        this.empSvc.getShiftDefinitions(),
      assignments: this.empSvc.getShiftAssignments(this.toDateStr(start), this.toDateStr(end)),
    }).subscribe({
      next: ({ defs, assignments }) => {
        this.shiftDefs.set(defs.data ?? []);
        this.shiftAssignments.set(assignments.data ?? []);
        this.shiftsLoading.set(false);
      },
      error: () => this.shiftsLoading.set(false),
    });
  }

  createShift(): void {
    if (this.shiftForm.invalid) { this.shiftForm.markAllAsTouched(); return; }
    this.empSvc.createShiftDefinition(this.shiftForm.value as CreateShiftDefinitionRequest).subscribe({
      next: () => { this.addingShift.set(false); this.shiftForm.reset({ colorCode: '#1976d2' }); this.notify.success('Shift created.'); this.loadShifts(); },
      error: err => this.notify.error(err?.error?.message || 'Failed.'),
    });
  }

  deleteShift(id: number): void {
    this.empSvc.deleteShiftDefinition(id).subscribe({
      next: () => { this.notify.success('Shift deleted.'); this.loadShifts(); },
      error: err => this.notify.error(err?.error?.message || 'Failed.'),
    });
  }

  setAssignmentUser(id: number): void   { this.newAssignment.update(a => ({ ...a, userId: id })); }
  setAssignmentShift(id: number): void  { this.newAssignment.update(a => ({ ...a, shiftId: id })); }
  setAssignmentDate(val: string): void  { this.newAssignment.update(a => ({ ...a, date: val })); }

  assignShift(): void {
    const a = this.newAssignment();
    if (!a.userId || !a.shiftId || !a.date) { this.notify.error('Fill all assignment fields.'); return; }
    this.empSvc.createShiftAssignment({ ...a }).subscribe({
      next: () => {
        this.newAssignment.set({ userId: 0, shiftId: 0, date: '' });
        this.notify.success('Shift assigned.');
        this.loadShifts();
      },
      error: err => this.notify.error(err?.error?.message || 'Failed.'),
    });
  }

  removeAssignment(id: number): void {
    this.empSvc.deleteShiftAssignment(id).subscribe({
      next: () => { this.notify.success('Assignment removed.'); this.loadShifts(); },
      error: () => this.notify.error('Failed.'),
    });
  }

  getAssignmentsForDay(date: Date): ShiftAssignment[] {
    const ds = this.toDateStr(date);
    return this.shiftAssignments().filter(a => a.date.startsWith(ds));
  }

  prevWeek(): void { const d = new Date(this.shiftWeekStart()); d.setDate(d.getDate() - 7); this.shiftWeekStart.set(d); this.loadShifts(); }
  nextWeek(): void { const d = new Date(this.shiftWeekStart()); d.setDate(d.getDate() + 7); this.shiftWeekStart.set(d); this.loadShifts(); }

  // ── Performance ────────────────────────────────────────────────────────

  loadReviews(): void {
    this.reviewsLoading.set(true);
    this.empSvc.getReviews().subscribe({
      next: res => { this.reviews.set(res.data ?? []); this.reviewsLoading.set(false); },
      error: () => this.reviewsLoading.set(false),
    });
  }

  submitReview(): void {
    if (this.reviewForm.invalid) { this.reviewForm.markAllAsTouched(); return; }
    this.empSvc.createReview(this.reviewForm.value as CreatePerformanceReviewRequest).subscribe({
      next: () => { this.addingReview.set(false); this.reviewForm.reset({ rating: 3, category: 'General', reviewDate: new Date().toISOString().substring(0, 10) }); this.notify.success('Review added.'); this.loadReviews(); },
      error: err => this.notify.error(err?.error?.message || 'Failed.'),
    });
  }

  deleteReview(id: number): void {
    this.empSvc.deleteReview(id).subscribe({
      next: () => { this.notify.success('Review deleted.'); this.loadReviews(); },
      error: () => this.notify.error('Failed.'),
    });
  }

  // ── Helpers ────────────────────────────────────────────────────────────

  getRoleIcon(n: string): string {
    const l = n?.toLowerCase() ?? '';
    if (l === 'admin')            return 'admin_panel_settings';
    if (l === 'cashier')          return 'point_of_sale';
    if (l === 'waiter')           return 'room_service';
    if (l === 'inventorymanager') return 'inventory_2';
    return 'badge';
  }

  getRoleClass(n: string): string { return (n ?? '').toLowerCase().replace(/\s+/g, ''); }

  private getMonday(d: Date): Date {
    const day = d.getDay();
    const diff = d.getDate() - day + (day === 0 ? -6 : 1);
    const m = new Date(d); m.setDate(diff); m.setHours(0, 0, 0, 0);
    return m;
  }

  toDateStr(d: Date): string {
    return d.toISOString().substring(0, 10);
  }

  stars(n: number): string[] { return Array(n).fill('star'); }
  emptyStars(n: number): string[] { return Array(5 - n).fill('star_border'); }
}

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
import { CheckboxModule } from 'primeng/checkbox';
import { CalendarModule } from 'primeng/calendar';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';

import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import {
  OrganizationApiService,
  OrganizationDto,
  UpdateOrganizationDto,
  BranchDto,
  CreateBranchDto,
  UpdateBranchDto,
  DepartmentDto,
  CreateDepartmentDto,
  UpdateDepartmentDto,
  DesignationDto,
  CreateDesignationDto,
  UpdateDesignationDto,
  JobGradeDto,
  CreateJobGradeDto,
  UpdateJobGradeDto,
  ShiftDto,
  CreateShiftDto,
  UpdateShiftDto,
  HolidayDto,
  CreateHolidayDto,
  UpdateHolidayDto
} from '../../core/organization-api.service';

@Component({
  selector: 'app-organization',
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
    CheckboxModule,
    CalendarModule,
    ToastModule,
    ConfirmDialogModule,
    HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './organization.component.html',
  styleUrl: './organization.component.scss'
})
export class OrganizationComponent implements OnInit {
  // ================= Company Profile =================
  profile: OrganizationDto | null = null;
  profileForm: UpdateOrganizationDto = this.emptyProfileForm();
  profileLoading = false;
  profileSaving = false;

  // ================= Branches =================
  branches: BranchDto[] = [];
  branchesLoading = false;
  showBranchDialog = false;
  editingBranch: BranchDto | null = null;
  branchForm: CreateBranchDto & { isActive?: boolean } = this.emptyBranchForm();
  branchSaving = false;

  // ================= Departments =================
  departments: DepartmentDto[] = [];
  departmentsLoading = false;
  showDepartmentDialog = false;
  editingDepartment: DepartmentDto | null = null;
  departmentForm: CreateDepartmentDto & { isActive?: boolean } = this.emptyDepartmentForm();
  departmentSaving = false;

  // ================= Designations =================
  designations: DesignationDto[] = [];
  designationsLoading = false;
  showDesignationDialog = false;
  editingDesignation: DesignationDto | null = null;
  designationForm: CreateDesignationDto & { isActive?: boolean } = this.emptyDesignationForm();
  designationSaving = false;

  jobGrades: JobGradeDto[] = [];
  jobGradesLoading = false;
  showJobGradesDialog = false;
  showJobGradeEditDialog = false;
  editingJobGrade: JobGradeDto | null = null;
  jobGradeForm: CreateJobGradeDto & { isActive?: boolean } = this.emptyJobGradeForm();
  jobGradeSaving = false;

  // ================= Shifts =================
  shifts: ShiftDto[] = [];
  shiftsLoading = false;
  showShiftDialog = false;
  editingShift: ShiftDto | null = null;
  shiftForm: {
    name: string;
    code: string;
    startTime: string; // "HH:mm" for <input type="time">
    endTime: string;
    breakMinutes: number;
    graceMinutes: number;
    isNightShift: boolean;
    weeklyOffDays: string;
    isActive?: boolean;
  } = this.emptyShiftForm();
  shiftSaving = false;

  // ================= Holidays =================
  holidays: HolidayDto[] = [];
  holidaysLoading = false;
  holidayYears: number[] = [];
  selectedHolidayYear: number | null = null;
  holidayTypes = ['National', 'Regional', 'Optional'];
  showHolidayDialog = false;
  editingHoliday: HolidayDto | null = null;
  holidayForm: { name: string; date: Date; type: string; description?: string | null; branchId?: number | null } =
    this.emptyHolidayForm();
  holidaySaving = false;

  constructor(
    private api: OrganizationApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {
    const currentYear = new Date().getFullYear();
    for (let y = currentYear - 2; y <= currentYear + 2; y++) this.holidayYears.push(y);
    this.selectedHolidayYear = currentYear;
  }

  ngOnInit() {
    this.loadProfile();
    this.loadBranches();
    this.loadDepartments();
    this.loadDesignations();
    this.loadJobGrades();
    this.loadShifts();
    this.loadHolidays();
  }

  // ==================================================================
  // Company Profile
  // ==================================================================
  private emptyProfileForm(): UpdateOrganizationDto {
    return {
      name: '',
      legalName: null,
      address: null,
      phone: null,
      email: null,
      website: null,
      taxNumber: null,
      timezone: 'Asia/Kolkata',
      currency: 'INR',
      logoUrl: null
    };
  }

  loadProfile() {
    this.profileLoading = true;
    this.api.getProfile().subscribe({
      next: p => {
        this.profile = p;
        this.profileForm = {
          name: p.name,
          legalName: p.legalName,
          address: p.address,
          phone: p.phone,
          email: p.email,
          website: p.website,
          taxNumber: p.taxNumber,
          timezone: p.timezone,
          currency: p.currency,
          logoUrl: p.logoUrl
        };
        this.profileLoading = false;
      },
      error: err => {
        this.profileLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load organization profile.');
      }
    });
  }

  saveProfile() {
    if (!this.profileForm.name.trim()) {
      this.notify.warn('Organization name is required.');
      return;
    }
    this.profileSaving = true;
    this.api.updateProfile(this.profileForm).subscribe({
      next: p => {
        this.profileSaving = false;
        this.profile = p;
        this.notify.success('Organization profile updated.');
      },
      error: err => {
        this.profileSaving = false;
        this.notify.error(err.error?.message ?? 'Failed to update organization profile.');
      }
    });
  }

  // ==================================================================
  // Branches
  // ==================================================================
  private emptyBranchForm(): CreateBranchDto & { isActive?: boolean } {
    return {
      name: '',
      code: '',
      address: null,
      city: null,
      state: null,
      country: null,
      phone: null,
      email: null,
      timezone: 'Asia/Kolkata',
      isHeadOffice: false
    };
  }

  loadBranches() {
    this.branchesLoading = true;
    this.api.getBranches().subscribe({
      next: rows => { this.branches = rows; this.branchesLoading = false; },
      error: err => {
        this.branchesLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load branches.');
      }
    });
  }

  openNewBranch() {
    this.editingBranch = null;
    this.branchForm = this.emptyBranchForm();
    this.showBranchDialog = true;
  }

  openEditBranch(branch: BranchDto) {
    this.editingBranch = branch;
    this.branchForm = {
      name: branch.name,
      code: branch.code,
      address: branch.address,
      city: branch.city,
      state: branch.state,
      country: branch.country,
      phone: branch.phone,
      email: branch.email,
      timezone: branch.timezone,
      isHeadOffice: branch.isHeadOffice,
      isActive: branch.isActive
    };
    this.showBranchDialog = true;
  }

  saveBranch() {
    if (!this.branchForm.name.trim() || !this.branchForm.code.trim()) {
      this.notify.warn('Name and code are required.');
      return;
    }
    this.branchSaving = true;
    const req$ = this.editingBranch
      ? this.api.updateBranch(this.editingBranch.id, { ...this.branchForm, isActive: this.branchForm.isActive ?? true } as UpdateBranchDto)
      : this.api.createBranch(this.branchForm);
    req$.subscribe({
      next: () => {
        this.branchSaving = false;
        this.showBranchDialog = false;
        this.notify.success(`Branch ${this.editingBranch ? 'updated' : 'created'}.`);
        this.loadBranches();
      },
      error: err => {
        this.branchSaving = false;
        this.notify.error(err.error?.message ?? 'Failed to save branch.');
      }
    });
  }

  deleteBranch(branch: BranchDto) {
    this.confirm.confirm({
      message: `Delete branch "${branch.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.deleteBranch(branch.id).subscribe({
          next: () => {
            this.notify.success('Branch deleted.');
            this.loadBranches();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete branch.')
        });
      }
    });
  }

  // ==================================================================
  // Departments
  // ==================================================================
  private emptyDepartmentForm(): CreateDepartmentDto & { isActive?: boolean } {
    return {
      branchId: null,
      parentId: null,
      name: '',
      code: '',
      description: null,
      headEmployeeId: null,
      costCenter: null
    };
  }

  loadDepartments() {
    this.departmentsLoading = true;
    this.api.getDepartments().subscribe({
      next: rows => { this.departments = rows; this.departmentsLoading = false; },
      error: err => {
        this.departmentsLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load departments.');
      }
    });
  }

  get parentDepartmentOptions(): DepartmentDto[] {
    return this.departments.filter(d => !this.editingDepartment || d.id !== this.editingDepartment.id);
  }

  openNewDepartment() {
    this.editingDepartment = null;
    this.departmentForm = this.emptyDepartmentForm();
    this.showDepartmentDialog = true;
  }

  openEditDepartment(dept: DepartmentDto) {
    this.editingDepartment = dept;
    this.departmentForm = {
      branchId: dept.branchId,
      parentId: dept.parentId,
      name: dept.name,
      code: dept.code,
      description: dept.description,
      headEmployeeId: dept.headEmployeeId,
      costCenter: dept.costCenter,
      isActive: dept.isActive
    };
    this.showDepartmentDialog = true;
  }

  saveDepartment() {
    if (!this.departmentForm.name.trim() || !this.departmentForm.code.trim()) {
      this.notify.warn('Name and code are required.');
      return;
    }
    this.departmentSaving = true;
    const req$ = this.editingDepartment
      ? this.api.updateDepartment(this.editingDepartment.id, {
          ...this.departmentForm,
          isActive: this.departmentForm.isActive ?? true
        } as UpdateDepartmentDto)
      : this.api.createDepartment(this.departmentForm);
    req$.subscribe({
      next: () => {
        this.departmentSaving = false;
        this.showDepartmentDialog = false;
        this.notify.success(`Department ${this.editingDepartment ? 'updated' : 'created'}.`);
        this.loadDepartments();
      },
      error: err => {
        this.departmentSaving = false;
        this.notify.error(err.error?.message ?? 'Failed to save department.');
      }
    });
  }

  deleteDepartment(dept: DepartmentDto) {
    this.confirm.confirm({
      message: `Delete department "${dept.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.deleteDepartment(dept.id).subscribe({
          next: () => {
            this.notify.success('Department deleted.');
            this.loadDepartments();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete department.')
        });
      }
    });
  }

  // ==================================================================
  // Designations
  // ==================================================================
  private emptyDesignationForm(): CreateDesignationDto & { isActive?: boolean } {
    return { title: '', code: '', jobGradeId: null, description: null };
  }

  loadDesignations() {
    this.designationsLoading = true;
    this.api.getDesignations().subscribe({
      next: rows => { this.designations = rows; this.designationsLoading = false; },
      error: err => {
        this.designationsLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load designations.');
      }
    });
  }

  openNewDesignation() {
    this.editingDesignation = null;
    this.designationForm = this.emptyDesignationForm();
    this.showDesignationDialog = true;
  }

  openEditDesignation(designation: DesignationDto) {
    this.editingDesignation = designation;
    this.designationForm = {
      title: designation.title,
      code: designation.code,
      jobGradeId: designation.jobGradeId,
      description: designation.description,
      isActive: designation.isActive
    };
    this.showDesignationDialog = true;
  }

  saveDesignation() {
    if (!this.designationForm.title.trim() || !this.designationForm.code.trim()) {
      this.notify.warn('Title and code are required.');
      return;
    }
    this.designationSaving = true;
    const req$ = this.editingDesignation
      ? this.api.updateDesignation(this.editingDesignation.id, {
          ...this.designationForm,
          isActive: this.designationForm.isActive ?? true
        } as UpdateDesignationDto)
      : this.api.createDesignation(this.designationForm);
    req$.subscribe({
      next: () => {
        this.designationSaving = false;
        this.showDesignationDialog = false;
        this.notify.success(`Designation ${this.editingDesignation ? 'updated' : 'created'}.`);
        this.loadDesignations();
      },
      error: err => {
        this.designationSaving = false;
        this.notify.error(err.error?.message ?? 'Failed to save designation.');
      }
    });
  }

  deleteDesignation(designation: DesignationDto) {
    this.confirm.confirm({
      message: `Delete designation "${designation.title}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.deleteDesignation(designation.id).subscribe({
          next: () => {
            this.notify.success('Designation deleted.');
            this.loadDesignations();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete designation.')
        });
      }
    });
  }

  // ---------------- Job Grades ----------------
  private emptyJobGradeForm(): CreateJobGradeDto & { isActive?: boolean } {
    return { name: '', level: 1, minAnnualSalary: 0, maxAnnualSalary: 0 };
  }

  loadJobGrades() {
    this.jobGradesLoading = true;
    this.api.getJobGrades().subscribe({
      next: rows => { this.jobGrades = rows; this.jobGradesLoading = false; },
      error: err => {
        this.jobGradesLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load job grades.');
      }
    });
  }

  openJobGradesManager() {
    this.showJobGradesDialog = true;
  }

  openNewJobGrade() {
    this.editingJobGrade = null;
    this.jobGradeForm = this.emptyJobGradeForm();
    this.showJobGradeEditDialog = true;
  }

  openEditJobGrade(grade: JobGradeDto) {
    this.editingJobGrade = grade;
    this.jobGradeForm = {
      name: grade.name,
      level: grade.level,
      minAnnualSalary: grade.minAnnualSalary,
      maxAnnualSalary: grade.maxAnnualSalary,
      isActive: grade.isActive
    };
    this.showJobGradeEditDialog = true;
  }

  saveJobGrade() {
    if (!this.jobGradeForm.name.trim()) {
      this.notify.warn('Name is required.');
      return;
    }
    this.jobGradeSaving = true;
    const req$ = this.editingJobGrade
      ? this.api.updateJobGrade(this.editingJobGrade.id, {
          ...this.jobGradeForm,
          isActive: this.jobGradeForm.isActive ?? true
        } as UpdateJobGradeDto)
      : this.api.createJobGrade(this.jobGradeForm);
    req$.subscribe({
      next: () => {
        this.jobGradeSaving = false;
        this.showJobGradeEditDialog = false;
        this.notify.success(`Job grade ${this.editingJobGrade ? 'updated' : 'created'}.`);
        this.loadJobGrades();
      },
      error: err => {
        this.jobGradeSaving = false;
        this.notify.error(err.error?.message ?? 'Failed to save job grade.');
      }
    });
  }

  deleteJobGrade(grade: JobGradeDto) {
    this.confirm.confirm({
      message: `Delete job grade "${grade.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.deleteJobGrade(grade.id).subscribe({
          next: () => {
            this.notify.success('Job grade deleted.');
            this.loadJobGrades();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete job grade.')
        });
      }
    });
  }

  // ==================================================================
  // Shifts
  // ==================================================================
  private emptyShiftForm() {
    return {
      name: '',
      code: '',
      startTime: '09:00',
      endTime: '18:00',
      breakMinutes: 60,
      graceMinutes: 10,
      isNightShift: false,
      weeklyOffDays: 'Saturday,Sunday'
    };
  }

  // Backend TimeOnly serializes as "HH:mm:ss"; <input type="time"> uses "HH:mm".
  private toTimeInput(value: string): string {
    return value ? value.substring(0, 5) : '';
  }

  private toTimeOnly(value: string): string {
    if (!value) return '00:00:00';
    return value.length === 5 ? `${value}:00` : value;
  }

  loadShifts() {
    this.shiftsLoading = true;
    this.api.getShifts().subscribe({
      next: rows => { this.shifts = rows; this.shiftsLoading = false; },
      error: err => {
        this.shiftsLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load shifts.');
      }
    });
  }

  openNewShift() {
    this.editingShift = null;
    this.shiftForm = this.emptyShiftForm();
    this.showShiftDialog = true;
  }

  openEditShift(shift: ShiftDto) {
    this.editingShift = shift;
    this.shiftForm = {
      name: shift.name,
      code: shift.code,
      startTime: this.toTimeInput(shift.startTime),
      endTime: this.toTimeInput(shift.endTime),
      breakMinutes: shift.breakMinutes,
      graceMinutes: shift.graceMinutes,
      isNightShift: shift.isNightShift,
      weeklyOffDays: shift.weeklyOffDays,
      isActive: shift.isActive
    };
    this.showShiftDialog = true;
  }

  saveShift() {
    if (!this.shiftForm.name.trim() || !this.shiftForm.code.trim()) {
      this.notify.warn('Name and code are required.');
      return;
    }
    this.shiftSaving = true;
    const payload = {
      name: this.shiftForm.name,
      code: this.shiftForm.code,
      startTime: this.toTimeOnly(this.shiftForm.startTime),
      endTime: this.toTimeOnly(this.shiftForm.endTime),
      breakMinutes: this.shiftForm.breakMinutes,
      graceMinutes: this.shiftForm.graceMinutes,
      isNightShift: this.shiftForm.isNightShift,
      weeklyOffDays: this.shiftForm.weeklyOffDays
    };
    const req$ = this.editingShift
      ? this.api.updateShift(this.editingShift.id, { ...payload, isActive: this.shiftForm.isActive ?? true } as UpdateShiftDto)
      : this.api.createShift(payload as CreateShiftDto);
    req$.subscribe({
      next: () => {
        this.shiftSaving = false;
        this.showShiftDialog = false;
        this.notify.success(`Shift ${this.editingShift ? 'updated' : 'created'}.`);
        this.loadShifts();
      },
      error: err => {
        this.shiftSaving = false;
        this.notify.error(err.error?.message ?? 'Failed to save shift.');
      }
    });
  }

  deleteShift(shift: ShiftDto) {
    this.confirm.confirm({
      message: `Delete shift "${shift.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.deleteShift(shift.id).subscribe({
          next: () => {
            this.notify.success('Shift deleted.');
            this.loadShifts();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete shift.')
        });
      }
    });
  }

  // ==================================================================
  // Holidays
  // ==================================================================
  private emptyHolidayForm() {
    return { name: '', date: new Date(), type: 'National', description: null as string | null, branchId: null as number | null };
  }

  private toDateOnly(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  loadHolidays() {
    this.holidaysLoading = true;
    this.api.getHolidays(this.selectedHolidayYear).subscribe({
      next: rows => { this.holidays = rows; this.holidaysLoading = false; },
      error: err => {
        this.holidaysLoading = false;
        this.notify.error(err.error?.message ?? 'Failed to load holidays.');
      }
    });
  }

  onHolidayYearChange() {
    this.loadHolidays();
  }

  openNewHoliday() {
    this.editingHoliday = null;
    this.holidayForm = this.emptyHolidayForm();
    this.showHolidayDialog = true;
  }

  openEditHoliday(holiday: HolidayDto) {
    this.editingHoliday = holiday;
    this.holidayForm = {
      name: holiday.name,
      date: new Date(holiday.date),
      type: holiday.type,
      description: holiday.description,
      branchId: holiday.branchId
    };
    this.showHolidayDialog = true;
  }

  saveHoliday() {
    if (!this.holidayForm.name.trim()) {
      this.notify.warn('Name is required.');
      return;
    }
    this.holidaySaving = true;
    const payload: CreateHolidayDto = {
      name: this.holidayForm.name,
      date: this.toDateOnly(this.holidayForm.date),
      type: this.holidayForm.type,
      description: this.holidayForm.description,
      branchId: this.holidayForm.branchId
    };
    const req$ = this.editingHoliday
      ? this.api.updateHoliday(this.editingHoliday.id, payload as UpdateHolidayDto)
      : this.api.createHoliday(payload);
    req$.subscribe({
      next: () => {
        this.holidaySaving = false;
        this.showHolidayDialog = false;
        this.notify.success(`Holiday ${this.editingHoliday ? 'updated' : 'created'}.`);
        this.loadHolidays();
      },
      error: err => {
        this.holidaySaving = false;
        this.notify.error(err.error?.message ?? 'Failed to save holiday.');
      }
    });
  }

  deleteHoliday(holiday: HolidayDto) {
    this.confirm.confirm({
      message: `Delete holiday "${holiday.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.deleteHoliday(holiday.id).subscribe({
          next: () => {
            this.notify.success('Holiday deleted.');
            this.loadHolidays();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete holiday.')
        });
      }
    });
  }
}

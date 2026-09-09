import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { TabViewModule } from 'primeng/tabview';
import { TableModule } from 'primeng/table';
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
import {
  EmployeesApiService,
  EmployeeDetailDto,
  UpdateEmployeeDto,
  EmployeeDocumentDto,
  EmployeeEducationDto,
  EmployeeExperienceDto,
  EmployeeFamilyMemberDto,
  LifecycleEventDto
} from '../../core/employees-api.service';

interface LookupOption {
  id: number;
  name: string;
}

@Component({
  selector: 'app-employee-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TabViewModule,
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
  templateUrl: './employee-detail.component.html',
  styleUrl: './employee-detail.component.scss'
})
export class EmployeeDetailComponent implements OnInit {
  employeeId!: number;
  employee: EmployeeDetailDto | null = null;
  loading = false;

  branches: LookupOption[] = [];
  departments: LookupOption[] = [];
  designations: LookupOption[] = [];
  shifts: LookupOption[] = [];

  genderOptions = [
    { label: 'Male', value: 'Male' },
    { label: 'Female', value: 'Female' },
    { label: 'Other', value: 'Other' }
  ];
  employmentTypeOptions = [
    { label: 'Full Time', value: 'FullTime' },
    { label: 'Part Time', value: 'PartTime' },
    { label: 'Contract', value: 'Contract' },
    { label: 'Intern', value: 'Intern' }
  ];
  statusOptions = [
    { label: 'Active', value: 'Active' },
    { label: 'Inactive', value: 'Inactive' },
    { label: 'Exited', value: 'Exited' }
  ];
  eventTypeOptions = [
    { label: 'Promotion', value: 'Promotion' },
    { label: 'Transfer', value: 'Transfer' },
    { label: 'Confirmation', value: 'Confirmation' },
    { label: 'Salary Revision', value: 'SalaryRevision' },
    { label: 'Other', value: 'Other' }
  ];

  showEditDialog = false;
  editForm: UpdateEmployeeDto = this.emptyUpdateForm();
  saving = false;

  showDocDialog = false;
  docForm: Partial<EmployeeDocumentDto> = {};

  showEduDialog = false;
  eduForm: Partial<EmployeeEducationDto> = {};

  showExpDialog = false;
  expForm: Partial<EmployeeExperienceDto> = {};

  showFamilyDialog = false;
  familyForm: Partial<EmployeeFamilyMemberDto> = { isEmergencyContact: false };

  showEventDialog = false;
  eventForm: Partial<LifecycleEventDto> = {};

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private employeesApi: EmployeesApiService,
    private http: HttpClient,
    private notify: NotificationService,
    private confirmation: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.employeeId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadLookups();
    this.load();
  }

  load() {
    this.loading = true;
    this.employeesApi.getById(this.employeeId).subscribe({
      next: data => {
        this.employee = data;
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load employee.');
      }
    });
  }

  loadLookups() {
    this.http.get<LookupOption[]>(`${environment.apiUrl}/branches`).subscribe({ next: d => (this.branches = d), error: () => (this.branches = []) });
    this.http.get<LookupOption[]>(`${environment.apiUrl}/departments`).subscribe({ next: d => (this.departments = d), error: () => (this.departments = []) });
    this.http.get<LookupOption[]>(`${environment.apiUrl}/designations`).subscribe({ next: d => (this.designations = d), error: () => (this.designations = []) });
    this.http.get<LookupOption[]>(`${environment.apiUrl}/shifts`).subscribe({ next: d => (this.shifts = d), error: () => (this.shifts = []) });
  }

  goBack() {
    this.router.navigate(['/employees']);
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

  // ---------- Profile edit ----------
  openEditDialog() {
    if (!this.employee) return;
    const e = this.employee;
    this.editForm = {
      branchId: e.branchId,
      departmentId: e.departmentId,
      designationId: e.designationId,
      shiftId: e.shiftId ?? null,
      managerId: e.managerId ?? null,
      firstName: e.firstName,
      lastName: e.lastName,
      gender: e.gender,
      dateOfBirth: e.dateOfBirth ?? null,
      maritalStatus: e.maritalStatus ?? null,
      bloodGroup: e.bloodGroup ?? null,
      nationality: e.nationality ?? null,
      workEmail: e.workEmail,
      personalEmail: e.personalEmail ?? null,
      phone: e.phone ?? null,
      currentAddress: e.currentAddress ?? null,
      permanentAddress: e.permanentAddress ?? null,
      photoUrl: e.photoUrl ?? null,
      joiningDate: e.joiningDate,
      confirmationDate: e.confirmationDate ?? null,
      exitDate: e.exitDate ?? null,
      employmentType: e.employmentType,
      status: e.status,
      nationalIdNumber: e.nationalIdNumber ?? null,
      taxIdNumber: e.taxIdNumber ?? null,
      pfNumber: e.pfNumber ?? null,
      esiNumber: e.esiNumber ?? null,
      passportNumber: e.passportNumber ?? null,
      passportExpiry: e.passportExpiry ?? null,
      bankName: e.bankName ?? null,
      bankAccountNumber: e.bankAccountNumber ?? null,
      bankIfscCode: e.bankIfscCode ?? null,
      skills: e.skills ?? null,
      languages: e.languages ?? null,
      notes: e.notes ?? null
    };
    this.showEditDialog = true;
  }

  saveProfile() {
    this.saving = true;
    this.employeesApi.update(this.employeeId, this.editForm).subscribe({
      next: () => {
        this.saving = false;
        this.showEditDialog = false;
        this.notify.success('Employee updated successfully.');
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to update employee.');
      }
    });
  }

  private emptyUpdateForm(): UpdateEmployeeDto {
    return {
      branchId: 0,
      departmentId: 0,
      designationId: 0,
      firstName: '',
      lastName: '',
      gender: 'Male',
      workEmail: '',
      joiningDate: new Date().toISOString().substring(0, 10),
      employmentType: 'FullTime',
      status: 'Active'
    };
  }

  // ---------- Documents ----------
  openAddDocument() {
    this.docForm = {};
    this.showDocDialog = true;
  }

  saveDocument() {
    this.employeesApi.addDocument(this.employeeId, this.docForm).subscribe({
      next: () => {
        this.showDocDialog = false;
        this.notify.success('Document added.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to add document.')
    });
  }

  deleteDocument(doc: EmployeeDocumentDto) {
    this.confirmation.confirm({
      message: `Delete document "${doc.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.employeesApi.removeDocument(this.employeeId, doc.id).subscribe({
          next: () => {
            this.notify.success('Document deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete document.')
        });
      }
    });
  }

  // ---------- Education ----------
  openAddEducation() {
    this.eduForm = {};
    this.showEduDialog = true;
  }

  saveEducation() {
    this.employeesApi.addEducation(this.employeeId, this.eduForm).subscribe({
      next: () => {
        this.showEduDialog = false;
        this.notify.success('Education added.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to add education.')
    });
  }

  deleteEducation(edu: EmployeeEducationDto) {
    this.confirmation.confirm({
      message: `Delete education "${edu.degree}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.employeesApi.removeEducation(this.employeeId, edu.id).subscribe({
          next: () => {
            this.notify.success('Education deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete education.')
        });
      }
    });
  }

  // ---------- Experience ----------
  openAddExperience() {
    this.expForm = {};
    this.showExpDialog = true;
  }

  saveExperience() {
    this.employeesApi.addExperience(this.employeeId, this.expForm).subscribe({
      next: () => {
        this.showExpDialog = false;
        this.notify.success('Experience added.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to add experience.')
    });
  }

  deleteExperience(exp: EmployeeExperienceDto) {
    this.confirmation.confirm({
      message: `Delete experience at "${exp.company}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.employeesApi.removeExperience(this.employeeId, exp.id).subscribe({
          next: () => {
            this.notify.success('Experience deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete experience.')
        });
      }
    });
  }

  // ---------- Family Members ----------
  openAddFamilyMember() {
    this.familyForm = { isEmergencyContact: false };
    this.showFamilyDialog = true;
  }

  saveFamilyMember() {
    this.employeesApi.addFamilyMember(this.employeeId, this.familyForm).subscribe({
      next: () => {
        this.showFamilyDialog = false;
        this.notify.success('Family member added.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to add family member.')
    });
  }

  deleteFamilyMember(member: EmployeeFamilyMemberDto) {
    this.confirmation.confirm({
      message: `Delete family member "${member.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.employeesApi.removeFamilyMember(this.employeeId, member.id).subscribe({
          next: () => {
            this.notify.success('Family member deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete family member.')
        });
      }
    });
  }

  // ---------- Timeline / Lifecycle Events ----------
  get lifecycleEventsSorted(): LifecycleEventDto[] {
    if (!this.employee) return [];
    return [...this.employee.lifecycleEvents].sort((a, b) => (a.eventDate < b.eventDate ? 1 : -1));
  }

  openAddEvent() {
    this.eventForm = { eventDate: new Date().toISOString().substring(0, 10) };
    this.showEventDialog = true;
  }

  saveEvent() {
    this.employeesApi.addLifecycleEvent(this.employeeId, this.eventForm).subscribe({
      next: () => {
        this.showEventDialog = false;
        this.notify.success('Lifecycle event added.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to add lifecycle event.')
    });
  }
}

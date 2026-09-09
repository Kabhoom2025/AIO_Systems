import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

// ---------------- Organization ----------------
export interface OrganizationDto {
  id: number;
  name: string;
  code: string;
  legalName?: string | null;
  address?: string | null;
  phone?: string | null;
  email?: string | null;
  website?: string | null;
  taxNumber?: string | null;
  timezone: string;
  currency: string;
  logoUrl?: string | null;
  isActive: boolean;
}

export interface UpdateOrganizationDto {
  name: string;
  legalName?: string | null;
  address?: string | null;
  phone?: string | null;
  email?: string | null;
  website?: string | null;
  taxNumber?: string | null;
  timezone: string;
  currency: string;
  logoUrl?: string | null;
}

// ---------------- Branch ----------------
export interface BranchDto {
  id: number;
  name: string;
  code: string;
  address?: string | null;
  city?: string | null;
  state?: string | null;
  country?: string | null;
  phone?: string | null;
  email?: string | null;
  timezone: string;
  isHeadOffice: boolean;
  isActive: boolean;
}

export interface CreateBranchDto {
  name: string;
  code: string;
  address?: string | null;
  city?: string | null;
  state?: string | null;
  country?: string | null;
  phone?: string | null;
  email?: string | null;
  timezone: string;
  isHeadOffice: boolean;
}

export interface UpdateBranchDto extends CreateBranchDto {
  isActive: boolean;
}

// ---------------- Department ----------------
export interface DepartmentDto {
  id: number;
  branchId?: number | null;
  branchName?: string | null;
  parentId?: number | null;
  parentName?: string | null;
  name: string;
  code: string;
  description?: string | null;
  headEmployeeId?: number | null;
  headEmployeeName?: string | null;
  costCenter?: string | null;
  employeeCount: number;
  isActive: boolean;
}

export interface CreateDepartmentDto {
  branchId?: number | null;
  parentId?: number | null;
  name: string;
  code: string;
  description?: string | null;
  headEmployeeId?: number | null;
  costCenter?: string | null;
}

export interface UpdateDepartmentDto extends CreateDepartmentDto {
  isActive: boolean;
}

// ---------------- Designation ----------------
export interface DesignationDto {
  id: number;
  title: string;
  code: string;
  jobGradeId?: number | null;
  jobGradeName?: string | null;
  description?: string | null;
  isActive: boolean;
}

export interface CreateDesignationDto {
  title: string;
  code: string;
  jobGradeId?: number | null;
  description?: string | null;
}

export interface UpdateDesignationDto extends CreateDesignationDto {
  isActive: boolean;
}

export interface JobGradeDto {
  id: number;
  name: string;
  level: number;
  minAnnualSalary: number;
  maxAnnualSalary: number;
  isActive: boolean;
}

export interface CreateJobGradeDto {
  name: string;
  level: number;
  minAnnualSalary: number;
  maxAnnualSalary: number;
}

export interface UpdateJobGradeDto extends CreateJobGradeDto {
  isActive: boolean;
}

// ---------------- Shift ----------------
export interface ShiftDto {
  id: number;
  name: string;
  code: string;
  startTime: string; // HH:mm:ss
  endTime: string; // HH:mm:ss
  breakMinutes: number;
  graceMinutes: number;
  isNightShift: boolean;
  weeklyOffDays: string;
  isActive: boolean;
}

export interface CreateShiftDto {
  name: string;
  code: string;
  startTime: string;
  endTime: string;
  breakMinutes: number;
  graceMinutes: number;
  isNightShift: boolean;
  weeklyOffDays: string;
}

export interface UpdateShiftDto extends CreateShiftDto {
  isActive: boolean;
}

// ---------------- Holiday ----------------
export interface HolidayDto {
  id: number;
  branchId?: number | null;
  branchName?: string | null;
  name: string;
  date: string;
  type: string;
  description?: string | null;
}

export interface CreateHolidayDto {
  branchId?: number | null;
  name: string;
  date: string;
  type: string;
  description?: string | null;
}

export interface UpdateHolidayDto extends CreateHolidayDto {}

@Injectable({ providedIn: 'root' })
export class OrganizationApiService {
  private orgUrl = `${environment.apiUrl}/organization`;
  private branchesUrl = `${environment.apiUrl}/branches`;
  private departmentsUrl = `${environment.apiUrl}/departments`;
  private designationsUrl = `${environment.apiUrl}/designations`;
  private jobGradesUrl = `${environment.apiUrl}/designations/job-grades`;
  private shiftsUrl = `${environment.apiUrl}/shifts`;
  private holidaysUrl = `${environment.apiUrl}/holidays`;

  constructor(private http: HttpClient) {}

  // Organization
  getProfile(): Observable<OrganizationDto> {
    return this.http.get<OrganizationDto>(this.orgUrl);
  }

  updateProfile(dto: UpdateOrganizationDto): Observable<OrganizationDto> {
    return this.http.put<OrganizationDto>(this.orgUrl, dto);
  }

  // Branches
  getBranches(): Observable<BranchDto[]> {
    return this.http.get<BranchDto[]>(this.branchesUrl);
  }

  getBranch(id: number): Observable<BranchDto> {
    return this.http.get<BranchDto>(`${this.branchesUrl}/${id}`);
  }

  createBranch(dto: CreateBranchDto): Observable<BranchDto> {
    return this.http.post<BranchDto>(this.branchesUrl, dto);
  }

  updateBranch(id: number, dto: UpdateBranchDto): Observable<BranchDto> {
    return this.http.put<BranchDto>(`${this.branchesUrl}/${id}`, dto);
  }

  deleteBranch(id: number): Observable<void> {
    return this.http.delete<void>(`${this.branchesUrl}/${id}`);
  }

  // Departments
  getDepartments(): Observable<DepartmentDto[]> {
    return this.http.get<DepartmentDto[]>(this.departmentsUrl);
  }

  getDepartment(id: number): Observable<DepartmentDto> {
    return this.http.get<DepartmentDto>(`${this.departmentsUrl}/${id}`);
  }

  createDepartment(dto: CreateDepartmentDto): Observable<DepartmentDto> {
    return this.http.post<DepartmentDto>(this.departmentsUrl, dto);
  }

  updateDepartment(id: number, dto: UpdateDepartmentDto): Observable<DepartmentDto> {
    return this.http.put<DepartmentDto>(`${this.departmentsUrl}/${id}`, dto);
  }

  deleteDepartment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.departmentsUrl}/${id}`);
  }

  // Designations
  getDesignations(): Observable<DesignationDto[]> {
    return this.http.get<DesignationDto[]>(this.designationsUrl);
  }

  getDesignation(id: number): Observable<DesignationDto> {
    return this.http.get<DesignationDto>(`${this.designationsUrl}/${id}`);
  }

  createDesignation(dto: CreateDesignationDto): Observable<DesignationDto> {
    return this.http.post<DesignationDto>(this.designationsUrl, dto);
  }

  updateDesignation(id: number, dto: UpdateDesignationDto): Observable<DesignationDto> {
    return this.http.put<DesignationDto>(`${this.designationsUrl}/${id}`, dto);
  }

  deleteDesignation(id: number): Observable<void> {
    return this.http.delete<void>(`${this.designationsUrl}/${id}`);
  }

  // Job Grades (nested under designations)
  getJobGrades(): Observable<JobGradeDto[]> {
    return this.http.get<JobGradeDto[]>(this.jobGradesUrl);
  }

  createJobGrade(dto: CreateJobGradeDto): Observable<JobGradeDto> {
    return this.http.post<JobGradeDto>(this.jobGradesUrl, dto);
  }

  updateJobGrade(id: number, dto: UpdateJobGradeDto): Observable<JobGradeDto> {
    return this.http.put<JobGradeDto>(`${this.jobGradesUrl}/${id}`, dto);
  }

  deleteJobGrade(id: number): Observable<void> {
    return this.http.delete<void>(`${this.jobGradesUrl}/${id}`);
  }

  // Shifts
  getShifts(): Observable<ShiftDto[]> {
    return this.http.get<ShiftDto[]>(this.shiftsUrl);
  }

  getShift(id: number): Observable<ShiftDto> {
    return this.http.get<ShiftDto>(`${this.shiftsUrl}/${id}`);
  }

  createShift(dto: CreateShiftDto): Observable<ShiftDto> {
    return this.http.post<ShiftDto>(this.shiftsUrl, dto);
  }

  updateShift(id: number, dto: UpdateShiftDto): Observable<ShiftDto> {
    return this.http.put<ShiftDto>(`${this.shiftsUrl}/${id}`, dto);
  }

  deleteShift(id: number): Observable<void> {
    return this.http.delete<void>(`${this.shiftsUrl}/${id}`);
  }

  // Holidays
  getHolidays(year?: number | null): Observable<HolidayDto[]> {
    let params = new HttpParams();
    if (year) params = params.set('year', year);
    return this.http.get<HolidayDto[]>(this.holidaysUrl, { params });
  }

  getUpcomingHolidays(): Observable<HolidayDto[]> {
    return this.http.get<HolidayDto[]>(`${this.holidaysUrl}/upcoming`);
  }

  getHoliday(id: number): Observable<HolidayDto> {
    return this.http.get<HolidayDto>(`${this.holidaysUrl}/${id}`);
  }

  createHoliday(dto: CreateHolidayDto): Observable<HolidayDto> {
    return this.http.post<HolidayDto>(this.holidaysUrl, dto);
  }

  updateHoliday(id: number, dto: UpdateHolidayDto): Observable<HolidayDto> {
    return this.http.put<HolidayDto>(`${this.holidaysUrl}/${id}`, dto);
  }

  deleteHoliday(id: number): Observable<void> {
    return this.http.delete<void>(`${this.holidaysUrl}/${id}`);
  }
}

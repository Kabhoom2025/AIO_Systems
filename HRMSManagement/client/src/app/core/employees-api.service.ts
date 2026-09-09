import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface EmployeeListDto {
  id: number;
  employeeCode: string;
  fullName: string;
  workEmail: string;
  phone?: string | null;
  photoUrl?: string | null;
  branchName: string;
  departmentName: string;
  designationTitle: string;
  managerName?: string | null;
  employmentType: string;
  status: string;
  joiningDate: string;
}

export interface EmployeeDocumentDto {
  id: number;
  type: string;
  name: string;
  filePath?: string | null;
  expiryDate?: string | null;
  notes?: string | null;
}

export interface EmployeeEducationDto {
  id: number;
  degree: string;
  institution: string;
  fieldOfStudy?: string | null;
  startYear?: number | null;
  endYear?: number | null;
  grade?: string | null;
}

export interface EmployeeExperienceDto {
  id: number;
  company: string;
  title: string;
  startDate?: string | null;
  endDate?: string | null;
  description?: string | null;
}

export interface EmployeeFamilyMemberDto {
  id: number;
  name: string;
  relationship: string;
  phone?: string | null;
  dateOfBirth?: string | null;
  isEmergencyContact: boolean;
}

export interface LifecycleEventDto {
  id: number;
  eventType: string;
  eventDate: string;
  fromValue?: string | null;
  toValue?: string | null;
  remarks?: string | null;
}

export interface EmployeeDetailDto {
  id: number;
  employeeCode: string;

  branchId: number;
  branchName: string;
  departmentId: number;
  departmentName: string;
  designationId: number;
  designationTitle: string;
  shiftId?: number | null;
  shiftName?: string | null;
  managerId?: number | null;
  managerName?: string | null;

  firstName: string;
  lastName: string;
  fullName: string;
  gender: string;
  dateOfBirth?: string | null;
  maritalStatus?: string | null;
  bloodGroup?: string | null;
  nationality?: string | null;

  workEmail: string;
  personalEmail?: string | null;
  phone?: string | null;
  currentAddress?: string | null;
  permanentAddress?: string | null;
  photoUrl?: string | null;

  joiningDate: string;
  confirmationDate?: string | null;
  exitDate?: string | null;
  employmentType: string;
  status: string;

  nationalIdNumber?: string | null;
  taxIdNumber?: string | null;
  pfNumber?: string | null;
  esiNumber?: string | null;
  passportNumber?: string | null;
  passportExpiry?: string | null;
  bankName?: string | null;
  bankAccountNumber?: string | null;
  bankIfscCode?: string | null;

  skills?: string | null;
  languages?: string | null;
  notes?: string | null;

  documents: EmployeeDocumentDto[];
  educations: EmployeeEducationDto[];
  experiences: EmployeeExperienceDto[];
  familyMembers: EmployeeFamilyMemberDto[];
  lifecycleEvents: LifecycleEventDto[];
}

export interface CreateEmployeeDto {
  branchId: number;
  departmentId: number;
  designationId: number;
  shiftId?: number | null;
  managerId?: number | null;

  firstName: string;
  lastName: string;
  gender: string;
  dateOfBirth?: string | null;
  maritalStatus?: string | null;
  bloodGroup?: string | null;
  nationality?: string | null;

  workEmail: string;
  personalEmail?: string | null;
  phone?: string | null;
  currentAddress?: string | null;
  permanentAddress?: string | null;
  photoUrl?: string | null;

  joiningDate: string;
  confirmationDate?: string | null;
  employmentType: string;

  nationalIdNumber?: string | null;
  taxIdNumber?: string | null;
  pfNumber?: string | null;
  esiNumber?: string | null;
  passportNumber?: string | null;
  passportExpiry?: string | null;
  bankName?: string | null;
  bankAccountNumber?: string | null;
  bankIfscCode?: string | null;

  skills?: string | null;
  languages?: string | null;
  notes?: string | null;
}

export interface UpdateEmployeeDto extends CreateEmployeeDto {
  exitDate?: string | null;
  status: string;
}

export interface EmployeeLookupDto {
  id: number;
  employeeCode: string;
  fullName: string;
  designationTitle: string;
}

@Injectable({ providedIn: 'root' })
export class EmployeesApiService {
  private baseUrl = `${environment.apiUrl}/employees`;

  constructor(private http: HttpClient) {}

  getPaged(
    page = 1,
    pageSize = 20,
    search?: string | null,
    departmentId?: number | null,
    branchId?: number | null,
    status?: string | null
  ): Observable<PagedResult<EmployeeListDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    if (departmentId) params = params.set('departmentId', departmentId);
    if (branchId) params = params.set('branchId', branchId);
    if (status) params = params.set('status', status);
    return this.http.get<PagedResult<EmployeeListDto>>(this.baseUrl, { params });
  }

  getLookup(): Observable<EmployeeLookupDto[]> {
    return this.http.get<EmployeeLookupDto[]>(`${this.baseUrl}/lookup`);
  }

  getMe(): Observable<EmployeeDetailDto> {
    return this.http.get<EmployeeDetailDto>(`${this.baseUrl}/me`);
  }

  getById(id: number): Observable<EmployeeDetailDto> {
    return this.http.get<EmployeeDetailDto>(`${this.baseUrl}/${id}`);
  }

  getTeam(id: number): Observable<EmployeeListDto[]> {
    return this.http.get<EmployeeListDto[]>(`${this.baseUrl}/${id}/team`);
  }

  getLifecycleEvents(id: number): Observable<LifecycleEventDto[]> {
    return this.http.get<LifecycleEventDto[]>(`${this.baseUrl}/${id}/lifecycle-events`);
  }

  create(dto: CreateEmployeeDto): Observable<EmployeeDetailDto> {
    return this.http.post<EmployeeDetailDto>(this.baseUrl, dto);
  }

  update(id: number, dto: UpdateEmployeeDto): Observable<EmployeeDetailDto> {
    return this.http.put<EmployeeDetailDto>(`${this.baseUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  addDocument(id: number, dto: Partial<EmployeeDocumentDto>): Observable<EmployeeDocumentDto> {
    return this.http.post<EmployeeDocumentDto>(`${this.baseUrl}/${id}/documents`, dto);
  }

  removeDocument(id: number, docId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}/documents/${docId}`);
  }

  addEducation(id: number, dto: Partial<EmployeeEducationDto>): Observable<EmployeeEducationDto> {
    return this.http.post<EmployeeEducationDto>(`${this.baseUrl}/${id}/educations`, dto);
  }

  removeEducation(id: number, eduId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}/educations/${eduId}`);
  }

  addExperience(id: number, dto: Partial<EmployeeExperienceDto>): Observable<EmployeeExperienceDto> {
    return this.http.post<EmployeeExperienceDto>(`${this.baseUrl}/${id}/experiences`, dto);
  }

  removeExperience(id: number, expId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}/experiences/${expId}`);
  }

  addFamilyMember(id: number, dto: Partial<EmployeeFamilyMemberDto>): Observable<EmployeeFamilyMemberDto> {
    return this.http.post<EmployeeFamilyMemberDto>(`${this.baseUrl}/${id}/family-members`, dto);
  }

  removeFamilyMember(id: number, memberId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}/family-members/${memberId}`);
  }

  addLifecycleEvent(id: number, dto: Partial<LifecycleEventDto>): Observable<LifecycleEventDto> {
    return this.http.post<LifecycleEventDto>(`${this.baseUrl}/${id}/lifecycle-events`, dto);
  }

  exportCsv(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/export`, { responseType: 'blob' });
  }
}

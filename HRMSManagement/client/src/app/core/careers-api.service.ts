import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PublicOrgDto {
  name: string;
  code: string;
}

export interface PublicJobOpeningDto {
  id: number;
  title: string;
  departmentName: string;
  location?: string | null;
  employmentType: string;
  minExperienceYears?: number | null;
  maxExperienceYears?: number | null;
  vacancies: number;
  postedDate: string;
}

export interface PublicJobOpeningDetailDto extends PublicJobOpeningDto {
  description?: string | null;
  designationTitle: string;
  salaryRangeFrom?: number | null;
  salaryRangeTo?: number | null;
}

export interface ApplyToJobDto {
  name: string;
  email: string;
  phone?: string | null;
  currentCompany?: string | null;
  totalExperienceYears?: number | null;
  expectedSalary?: number | null;
  resumeUrl?: string | null;
  source?: string | null;
  coverNote?: string | null;
}

export interface ApplyResultDto {
  candidateId: number;
  trackingToken: string;
  message: string;
}

export interface TrackingStageDto {
  stage: string;
  isDone: boolean;
  isCurrent: boolean;
}

export interface StageTimelineEventDto {
  stage: string;
  notes?: string | null;
  changedByName: string;
  changedAt: string;
}

export interface TrackingStatusDto {
  candidateName: string;
  jobTitle: string;
  companyName: string;
  currentStage: string;
  isRejected: boolean;
  isHired: boolean;
  progressPercent: number;
  appliedDate: string;
  stages: TrackingStageDto[];
  timeline: StageTimelineEventDto[];
}

@Injectable({ providedIn: 'root' })
export class CareersApiService {
  private baseUrl = `${environment.apiUrl}/careers`;

  constructor(private http: HttpClient) {}

  getOrg(orgCode: string): Observable<PublicOrgDto> {
    return this.http.get<PublicOrgDto>(`${this.baseUrl}/org/${orgCode}`);
  }

  getOpenings(orgCode: string): Observable<PublicJobOpeningDto[]> {
    return this.http.get<PublicJobOpeningDto[]>(`${this.baseUrl}/org/${orgCode}/openings`);
  }

  getOpening(orgCode: string, id: number): Observable<PublicJobOpeningDetailDto> {
    return this.http.get<PublicJobOpeningDetailDto>(`${this.baseUrl}/org/${orgCode}/openings/${id}`);
  }

  apply(orgCode: string, id: number, dto: ApplyToJobDto): Observable<ApplyResultDto> {
    return this.http.post<ApplyResultDto>(`${this.baseUrl}/org/${orgCode}/openings/${id}/apply`, dto);
  }

  track(token: string): Observable<TrackingStatusDto> {
    return this.http.get<TrackingStatusDto>(`${this.baseUrl}/track/${token}`);
  }
}

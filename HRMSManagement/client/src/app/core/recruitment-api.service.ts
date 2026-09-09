import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface JobOpeningDto {
  id: number;
  departmentId: number;
  departmentName: string;
  designationId: number;
  designationTitle: string;
  title: string;
  description?: string | null;
  vacancies: number;
  location?: string | null;
  employmentType: string;
  minExperienceYears?: number | null;
  maxExperienceYears?: number | null;
  salaryRangeFrom?: number | null;
  salaryRangeTo?: number | null;
  status: string;
  postedDate: string;
  closingDate?: string | null;
  candidateCount: number;
}

export interface CreateJobOpeningDto {
  departmentId: number;
  designationId: number;
  title: string;
  description?: string | null;
  vacancies: number;
  location?: string | null;
  employmentType: string;
  minExperienceYears?: number | null;
  maxExperienceYears?: number | null;
  salaryRangeFrom?: number | null;
  salaryRangeTo?: number | null;
  postedDate: string;
  closingDate?: string | null;
}

export interface UpdateJobOpeningDto extends CreateJobOpeningDto {
  status: string;
}

export interface CandidateDto {
  id: number;
  jobOpeningId: number;
  jobOpeningTitle: string;
  name: string;
  email: string;
  phone?: string | null;
  resumeUrl?: string | null;
  currentCompany?: string | null;
  totalExperienceYears?: number | null;
  expectedSalary?: number | null;
  source?: string | null;
  stage: string;
  rating?: number | null;
  offeredSalary?: number | null;
  offerDate?: string | null;
  expectedJoiningDate?: string | null;
  notes?: string | null;
}

export interface StageHistoryDto {
  stage: string;
  notes?: string | null;
  changedByName: string;
  changedAt: string;
}

export interface CandidateDetailDto extends CandidateDto {
  interviews: InterviewDto[];
  stageHistory: StageHistoryDto[];
  progressPercent: number;
}

export interface CreateCandidateDto {
  name: string;
  email: string;
  phone?: string | null;
  resumeUrl?: string | null;
  currentCompany?: string | null;
  totalExperienceYears?: number | null;
  expectedSalary?: number | null;
  source?: string | null;
  rating?: number | null;
  notes?: string | null;
}

export interface UpdateCandidateDto extends CreateCandidateDto {}

export interface ChangeCandidateStageDto {
  stage: string;
  notes?: string | null;
  offeredSalary?: number | null;
  offerDate?: string | null;
  expectedJoiningDate?: string | null;
}

export interface InterviewDto {
  id: number;
  candidateId: number;
  candidateName: string;
  jobOpeningTitle: string;
  round: number;
  title: string;
  scheduledAt: string;
  mode: string;
  interviewerName?: string | null;
  interviewerUserId?: number | null;
  status: string;
  feedback?: string | null;
  score?: number | null;
}

export interface CreateInterviewDto {
  round: number;
  title: string;
  scheduledAt: string;
  mode: string;
  interviewerName?: string | null;
  interviewerUserId?: number | null;
}

export interface UpdateInterviewDto extends CreateInterviewDto {}

export interface InterviewFeedbackDto {
  status: string;
  feedback?: string | null;
  score?: number | null;
}

export interface RecruitmentPipelineDto {
  applied: number;
  screening: number;
  interview: number;
  offered: number;
  hired: number;
  rejected: number;
}

@Injectable({ providedIn: 'root' })
export class RecruitmentApiService {
  private baseUrl = `${environment.apiUrl}/recruitment`;

  constructor(private http: HttpClient) {}

  // Job openings
  getOpenings(status?: string | null): Observable<JobOpeningDto[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    return this.http.get<JobOpeningDto[]>(`${this.baseUrl}/openings`, { params });
  }

  getOpening(id: number): Observable<JobOpeningDto> {
    return this.http.get<JobOpeningDto>(`${this.baseUrl}/openings/${id}`);
  }

  createOpening(dto: CreateJobOpeningDto): Observable<JobOpeningDto> {
    return this.http.post<JobOpeningDto>(`${this.baseUrl}/openings`, dto);
  }

  updateOpening(id: number, dto: UpdateJobOpeningDto): Observable<JobOpeningDto> {
    return this.http.put<JobOpeningDto>(`${this.baseUrl}/openings/${id}`, dto);
  }

  deleteOpening(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/openings/${id}`);
  }

  // Candidates
  getCandidates(openingId: number): Observable<CandidateDto[]> {
    return this.http.get<CandidateDto[]>(`${this.baseUrl}/openings/${openingId}/candidates`);
  }

  getCandidate(id: number): Observable<CandidateDetailDto> {
    return this.http.get<CandidateDetailDto>(`${this.baseUrl}/candidates/${id}`);
  }

  createCandidate(openingId: number, dto: CreateCandidateDto): Observable<CandidateDto> {
    return this.http.post<CandidateDto>(`${this.baseUrl}/openings/${openingId}/candidates`, dto);
  }

  updateCandidate(id: number, dto: UpdateCandidateDto): Observable<CandidateDto> {
    return this.http.put<CandidateDto>(`${this.baseUrl}/candidates/${id}`, dto);
  }

  changeStage(id: number, dto: ChangeCandidateStageDto): Observable<CandidateDto> {
    return this.http.post<CandidateDto>(`${this.baseUrl}/candidates/${id}/stage`, dto);
  }

  deleteCandidate(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/candidates/${id}`);
  }

  // Interviews
  getUpcomingInterviews(): Observable<InterviewDto[]> {
    return this.http.get<InterviewDto[]>(`${this.baseUrl}/interviews`);
  }

  scheduleInterview(candidateId: number, dto: CreateInterviewDto): Observable<InterviewDto> {
    return this.http.post<InterviewDto>(`${this.baseUrl}/candidates/${candidateId}/interviews`, dto);
  }

  updateInterview(id: number, dto: UpdateInterviewDto): Observable<InterviewDto> {
    return this.http.put<InterviewDto>(`${this.baseUrl}/interviews/${id}`, dto);
  }

  submitInterviewFeedback(id: number, dto: InterviewFeedbackDto): Observable<InterviewDto> {
    return this.http.post<InterviewDto>(`${this.baseUrl}/interviews/${id}/feedback`, dto);
  }

  // Pipeline
  getPipeline(): Observable<RecruitmentPipelineDto> {
    return this.http.get<RecruitmentPipelineDto>(`${this.baseUrl}/pipeline`);
  }
}

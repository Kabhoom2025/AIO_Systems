import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PerformanceGoalDto {
  id: number;
  employeeId: number;
  employeeName: string;
  title: string;
  description?: string | null;
  metric?: string | null;
  weight: number;
  startDate: string;
  dueDate: string;
  progressPercent: number;
  status: string;
}

export interface CreatePerformanceGoalDto {
  employeeId: number;
  title: string;
  description?: string | null;
  metric?: string | null;
  weight: number;
  startDate: string;
  dueDate: string;
}

export interface UpdatePerformanceGoalDto {
  title: string;
  description?: string | null;
  metric?: string | null;
  weight: number;
  startDate: string;
  dueDate: string;
  status: string;
}

export interface UpdateGoalProgressDto {
  progressPercent: number;
  status?: string | null;
}

export interface PerformanceReviewDto {
  id: number;
  employeeId: number;
  employeeName: string;
  reviewerUserId?: number | null;
  reviewerName?: string | null;
  period: string;
  selfRating?: number | null;
  selfComments?: string | null;
  managerRating?: number | null;
  managerComments?: string | null;
  finalRating?: number | null;
  status: string;
  recommendation: string;
  completedAt?: string | null;
}

export interface CreatePerformanceReviewDto {
  employeeId: number;
  period: string;
  reviewerUserId?: number | null;
}

export interface SelfReviewDto {
  selfRating: number;
  selfComments: string;
}

export interface ManagerReviewDto {
  managerRating: number;
  managerComments: string;
  recommendation: string;
}

@Injectable({ providedIn: 'root' })
export class PerformanceApiService {
  private baseUrl = `${environment.apiUrl}/performance`;

  constructor(private http: HttpClient) {}

  // Goals
  getGoalsByEmployee(employeeId: number): Observable<PerformanceGoalDto[]> {
    return this.http.get<PerformanceGoalDto[]>(`${this.baseUrl}/goals/employee/${employeeId}`);
  }

  getMyGoals(): Observable<PerformanceGoalDto[]> {
    return this.http.get<PerformanceGoalDto[]>(`${this.baseUrl}/goals/my`);
  }

  createGoal(dto: CreatePerformanceGoalDto): Observable<PerformanceGoalDto> {
    return this.http.post<PerformanceGoalDto>(`${this.baseUrl}/goals`, dto);
  }

  updateGoal(id: number, dto: UpdatePerformanceGoalDto): Observable<PerformanceGoalDto> {
    return this.http.put<PerformanceGoalDto>(`${this.baseUrl}/goals/${id}`, dto);
  }

  updateGoalProgress(id: number, dto: UpdateGoalProgressDto): Observable<PerformanceGoalDto> {
    return this.http.post<PerformanceGoalDto>(`${this.baseUrl}/goals/${id}/progress`, dto);
  }

  deleteGoal(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/goals/${id}`);
  }

  // Reviews
  getReviews(period?: string | null, status?: string | null): Observable<PerformanceReviewDto[]> {
    let params = new HttpParams();
    if (period) params = params.set('period', period);
    if (status) params = params.set('status', status);
    return this.http.get<PerformanceReviewDto[]>(`${this.baseUrl}/reviews`, { params });
  }

  getMyReviews(): Observable<PerformanceReviewDto[]> {
    return this.http.get<PerformanceReviewDto[]>(`${this.baseUrl}/reviews/my`);
  }

  getReview(id: number): Observable<PerformanceReviewDto> {
    return this.http.get<PerformanceReviewDto>(`${this.baseUrl}/reviews/${id}`);
  }

  createReview(dto: CreatePerformanceReviewDto): Observable<PerformanceReviewDto> {
    return this.http.post<PerformanceReviewDto>(`${this.baseUrl}/reviews`, dto);
  }

  submitSelfReview(id: number, dto: SelfReviewDto): Observable<PerformanceReviewDto> {
    return this.http.post<PerformanceReviewDto>(`${this.baseUrl}/reviews/${id}/self`, dto);
  }

  submitManagerReview(id: number, dto: ManagerReviewDto): Observable<PerformanceReviewDto> {
    return this.http.post<PerformanceReviewDto>(`${this.baseUrl}/reviews/${id}/manager`, dto);
  }

  deleteReview(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/reviews/${id}`);
  }
}

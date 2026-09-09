import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ScheduledJobDefinitionDto {
  id: number;
  organizationId?: number | null;
  name: string;
  jobKey: string;
  cronExpression: string;
  isEnabled: boolean;
  lastRunAt?: string | null;
  lastRunStatus?: string | null;
}

export interface UpdateScheduledJobDto {
  cronExpression: string;
  isEnabled: boolean;
}

@Injectable({ providedIn: 'root' })
export class ScheduledJobApiService {
  private jobsUrl = `${environment.apiUrl}/scheduled-jobs`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ScheduledJobDefinitionDto[]> {
    return this.http.get<ScheduledJobDefinitionDto[]>(this.jobsUrl);
  }

  update(id: number, dto: UpdateScheduledJobDto): Observable<ScheduledJobDefinitionDto> {
    return this.http.put<ScheduledJobDefinitionDto>(`${this.jobsUrl}/${id}`, dto);
  }

  triggerNow(id: number): Observable<{ queuedJobId: string }> {
    return this.http.post<{ queuedJobId: string }>(`${this.jobsUrl}/${id}/trigger-now`, {});
  }
}

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ProjectDto {
  id: number;
  code: string;
  name: string;
  description: string | null;
  managerId: number;
  managerName: string;

  startDate: string;
  endDate: string | null;
  budget: number | null;
  status: string;

  taskCount: number;
  completedTaskCount: number;
}

export interface CreateProjectDto {
  code: string;
  name: string;
  description: string | null;
  managerId: number;

  startDate: string;
  endDate: string | null;
  budget: number | null;
}

export interface UpdateProjectDto {
  name: string;
  description: string | null;
  managerId: number;

  startDate: string;
  endDate: string | null;
  budget: number | null;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class ProjectApiService {
  private projectsUrl = `${environment.apiUrl}/projects`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ProjectDto[]> {
    return this.http.get<ProjectDto[]>(this.projectsUrl);
  }

  getById(id: number): Observable<ProjectDto> {
    return this.http.get<ProjectDto>(`${this.projectsUrl}/${id}`);
  }

  create(dto: CreateProjectDto): Observable<ProjectDto> {
    return this.http.post<ProjectDto>(this.projectsUrl, dto);
  }

  update(id: number, dto: UpdateProjectDto): Observable<ProjectDto> {
    return this.http.put<ProjectDto>(`${this.projectsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.projectsUrl}/${id}`);
  }
}

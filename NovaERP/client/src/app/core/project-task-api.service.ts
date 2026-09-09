import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ProjectTaskDto {
  id: number;
  projectId: number;
  projectName: string;
  assignedToId: number | null;
  assignedToName: string | null;

  title: string;
  description: string | null;
  priority: string;
  status: string;

  startDate: string | null;
  dueDate: string | null;
}

export interface CreateProjectTaskDto {
  projectId: number;
  assignedToId: number | null;

  title: string;
  description: string | null;
  priority: string;
  status: string;

  startDate: string | null;
  dueDate: string | null;
}

export interface UpdateProjectTaskDto {
  assignedToId: number | null;

  title: string;
  description: string | null;
  priority: string;
  status: string;

  startDate: string | null;
  dueDate: string | null;
}

@Injectable({ providedIn: 'root' })
export class ProjectTaskApiService {
  private tasksUrl = `${environment.apiUrl}/project-tasks`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ProjectTaskDto[]> {
    return this.http.get<ProjectTaskDto[]>(this.tasksUrl);
  }

  getById(id: number): Observable<ProjectTaskDto> {
    return this.http.get<ProjectTaskDto>(`${this.tasksUrl}/${id}`);
  }

  create(dto: CreateProjectTaskDto): Observable<ProjectTaskDto> {
    return this.http.post<ProjectTaskDto>(this.tasksUrl, dto);
  }

  update(id: number, dto: UpdateProjectTaskDto): Observable<ProjectTaskDto> {
    return this.http.put<ProjectTaskDto>(`${this.tasksUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.tasksUrl}/${id}`);
  }
}

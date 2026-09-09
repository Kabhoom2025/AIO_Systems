import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ServiceTicketDto {
  id: number;
  ticketNumber: string;
  subject: string;
  description: string;
  categoryId: number;
  categoryName: string;
  requesterId: number;
  requesterName: string;
  assignedToId: number | null;
  assignedToName: string | null;

  priority: string;
  status: string;
  resolutionNotes: string | null;
  resolvedDate: string | null;
  closedDate: string | null;
}

export interface CreateServiceTicketDto {
  subject: string;
  description: string;
  categoryId: number;
  requesterId: number;
  priority: string;
}

export interface UpdateServiceTicketDto {
  subject: string;
  description: string;
  categoryId: number;
  priority: string;
}

export interface AssignTicketDto {
  employeeId: number;
}

export interface ResolveTicketDto {
  resolutionNotes: string;
}

@Injectable({ providedIn: 'root' })
export class ServiceTicketApiService {
  private ticketsUrl = `${environment.apiUrl}/service-tickets`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ServiceTicketDto[]> {
    return this.http.get<ServiceTicketDto[]>(this.ticketsUrl);
  }

  getById(id: number): Observable<ServiceTicketDto> {
    return this.http.get<ServiceTicketDto>(`${this.ticketsUrl}/${id}`);
  }

  create(dto: CreateServiceTicketDto): Observable<ServiceTicketDto> {
    return this.http.post<ServiceTicketDto>(this.ticketsUrl, dto);
  }

  update(id: number, dto: UpdateServiceTicketDto): Observable<ServiceTicketDto> {
    return this.http.put<ServiceTicketDto>(`${this.ticketsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.ticketsUrl}/${id}`);
  }

  assign(id: number, dto: AssignTicketDto): Observable<ServiceTicketDto> {
    return this.http.post<ServiceTicketDto>(`${this.ticketsUrl}/${id}/assign`, dto);
  }

  resolve(id: number, dto: ResolveTicketDto): Observable<ServiceTicketDto> {
    return this.http.post<ServiceTicketDto>(`${this.ticketsUrl}/${id}/resolve`, dto);
  }

  close(id: number): Observable<ServiceTicketDto> {
    return this.http.post<ServiceTicketDto>(`${this.ticketsUrl}/${id}/close`, {});
  }

  reopen(id: number): Observable<ServiceTicketDto> {
    return this.http.post<ServiceTicketDto>(`${this.ticketsUrl}/${id}/reopen`, {});
  }
}

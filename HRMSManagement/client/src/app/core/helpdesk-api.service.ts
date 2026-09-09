import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface TicketDto {
  id: number;
  ticketNumber: string;
  category: string;
  priority: string;
  subject: string;
  description: string;
  status: string;
  raisedByEmployeeId: number;
  raisedByName: string;
  assignedToUserId: number | null;
  assignedToName: string | null;
  slaDueAt: string | null;
  resolvedAt: string | null;
  commentCount: number;
  createdDate: string;
}

export interface TicketCommentDto {
  id: number;
  authorUserId: number | null;
  authorName: string;
  comment: string;
  createdDate: string;
}

export interface TicketDetailDto extends TicketDto {
  comments: TicketCommentDto[];
}

export interface CreateTicketDto {
  category: string;
  priority: string;
  subject: string;
  description: string;
}

export interface AddCommentDto {
  comment: string;
}

export interface AssignTicketDto {
  userId: number;
}

export interface UpdateTicketStatusDto {
  status: string;
}

export interface HelpDeskSummaryDto {
  open: number;
  inProgress: number;
  resolved: number;
  closed: number;
  breachedSla: number;
}

@Injectable({ providedIn: 'root' })
export class HelpDeskApiService {
  private base = `${environment.apiUrl}/helpdesk`;

  constructor(private http: HttpClient) {}

  createTicket(dto: CreateTicketDto): Observable<TicketDto> {
    return this.http.post<TicketDto>(`${this.base}/tickets`, dto);
  }

  myTickets(): Observable<TicketDto[]> {
    return this.http.get<TicketDto[]>(`${this.base}/tickets/my`);
  }

  getAll(status?: string | null, category?: string | null): Observable<TicketDto[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    if (category) params = params.set('category', category);
    return this.http.get<TicketDto[]>(`${this.base}/tickets`, { params });
  }

  summary(): Observable<HelpDeskSummaryDto> {
    return this.http.get<HelpDeskSummaryDto>(`${this.base}/summary`);
  }

  getById(id: number): Observable<TicketDetailDto> {
    return this.http.get<TicketDetailDto>(`${this.base}/tickets/${id}`);
  }

  addComment(id: number, dto: AddCommentDto): Observable<TicketDetailDto> {
    return this.http.post<TicketDetailDto>(`${this.base}/tickets/${id}/comments`, dto);
  }

  assign(id: number, dto: AssignTicketDto): Observable<TicketDto> {
    return this.http.post<TicketDto>(`${this.base}/tickets/${id}/assign`, dto);
  }

  updateStatus(id: number, dto: UpdateTicketStatusDto): Observable<TicketDto> {
    return this.http.post<TicketDto>(`${this.base}/tickets/${id}/status`, dto);
  }
}

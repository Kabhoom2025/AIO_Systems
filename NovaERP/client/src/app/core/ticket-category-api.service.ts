import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface TicketCategoryDto {
  id: number;
  name: string;
  code: string;
  isActive: boolean;
}

export interface CreateTicketCategoryDto {
  name: string;
  code: string;
  isActive: boolean;
}

export interface UpdateTicketCategoryDto {
  name: string;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class TicketCategoryApiService {
  private categoriesUrl = `${environment.apiUrl}/ticket-categories`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<TicketCategoryDto[]> {
    return this.http.get<TicketCategoryDto[]>(this.categoriesUrl);
  }

  getById(id: number): Observable<TicketCategoryDto> {
    return this.http.get<TicketCategoryDto>(`${this.categoriesUrl}/${id}`);
  }

  create(dto: CreateTicketCategoryDto): Observable<TicketCategoryDto> {
    return this.http.post<TicketCategoryDto>(this.categoriesUrl, dto);
  }

  update(id: number, dto: UpdateTicketCategoryDto): Observable<TicketCategoryDto> {
    return this.http.put<TicketCategoryDto>(`${this.categoriesUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.categoriesUrl}/${id}`);
  }
}

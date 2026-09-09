import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ReservationDto,
  CreateReservationRequest,
  UpdateReservationRequest,
  UpdateReservationStatusRequest,
} from '../models/reservation.model';

interface ApiResponse<T> { success: boolean; data: T; message: string; }

@Injectable({ providedIn: 'root' })
export class ReservationService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/reservations`;

  getAll(from?: string, to?: string): Observable<ApiResponse<ReservationDto[]>> {
    let params = new HttpParams();
    if (from) params = params.set('from', from);
    if (to)   params = params.set('to', to);
    return this.http.get<ApiResponse<ReservationDto[]>>(this.base, { params });
  }

  getUpcoming(): Observable<ApiResponse<ReservationDto[]>> {
    return this.http.get<ApiResponse<ReservationDto[]>>(`${this.base}/upcoming`);
  }

  getByTable(tableId: number): Observable<ApiResponse<ReservationDto[]>> {
    return this.http.get<ApiResponse<ReservationDto[]>>(`${this.base}/table/${tableId}`);
  }

  create(dto: CreateReservationRequest): Observable<ApiResponse<ReservationDto>> {
    return this.http.post<ApiResponse<ReservationDto>>(this.base, dto);
  }

  update(id: number, dto: UpdateReservationRequest): Observable<ApiResponse<ReservationDto>> {
    return this.http.put<ApiResponse<ReservationDto>>(`${this.base}/${id}`, dto);
  }

  updateStatus(id: number, dto: UpdateReservationStatusRequest): Observable<ApiResponse<ReservationDto>> {
    return this.http.patch<ApiResponse<ReservationDto>>(`${this.base}/${id}/status`, dto);
  }

  delete(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.base}/${id}`);
  }
}

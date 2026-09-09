import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { AddOn } from '../models/addon.model';

export interface CreateAddOnRequest {
  name: string;
  price: number;
  category?: string;
  isAvailable: boolean;
}

@Injectable({ providedIn: 'root' })
export class AddOnService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/addon`;

  getAll(): Observable<ApiResponse<AddOn[]>> {
    return this.http.get<ApiResponse<AddOn[]>>(this.apiUrl);
  }

  getByFoodItem(foodItemId: number): Observable<ApiResponse<AddOn[]>> {
    return this.http.get<ApiResponse<AddOn[]>>(`${this.apiUrl}/by-item/${foodItemId}`);
  }

  create(payload: CreateAddOnRequest): Observable<ApiResponse<AddOn>> {
    return this.http.post<ApiResponse<AddOn>>(this.apiUrl, payload);
  }

  update(id: number, payload: CreateAddOnRequest): Observable<ApiResponse<AddOn>> {
    return this.http.put<ApiResponse<AddOn>>(`${this.apiUrl}/${id}`, payload);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.apiUrl}/${id}`);
  }
}

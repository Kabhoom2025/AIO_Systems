import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { FoodItem, CreateFoodItemRequest, UpdateFoodItemRequest } from '../models/food-item.model';

@Injectable({ providedIn: 'root' })
export class FoodItemService {
  private readonly apiUrl = `${environment.apiUrl}/fooditem`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<FoodItem[]>> {
    return this.http.get<ApiResponse<FoodItem[]>>(this.apiUrl);
  }

  getById(id: number): Observable<ApiResponse<FoodItem>> {
    return this.http.get<ApiResponse<FoodItem>>(`${this.apiUrl}/${id}`);
  }

  getByCategory(categoryId: number): Observable<ApiResponse<FoodItem[]>> {
    return this.http.get<ApiResponse<FoodItem[]>>(`${this.apiUrl}/by-category/${categoryId}`);
  }

  create(payload: CreateFoodItemRequest): Observable<ApiResponse<FoodItem>> {
    return this.http.post<ApiResponse<FoodItem>>(this.apiUrl, payload);
  }

  update(id: number, payload: UpdateFoodItemRequest): Observable<ApiResponse<FoodItem>> {
    return this.http.put<ApiResponse<FoodItem>>(`${this.apiUrl}/${id}`, payload);
  }

  delete(id: number): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }

  getByBarcode(code: string): Observable<ApiResponse<FoodItem>> {
    return this.http.get<ApiResponse<FoodItem>>(`${this.apiUrl}/barcode/${encodeURIComponent(code)}`);
  }
}

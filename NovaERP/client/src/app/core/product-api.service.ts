import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ProductDto {
  id: number;
  sku: string;
  name: string;
  description: string | null;
  unitOfMeasure: string;
  unitCost: number;
  weightKg: number | null;
  isActive: boolean;
  onHandQuantity: number;
}

export interface CreateProductDto {
  sku: string;
  name: string;
  description: string | null;
  unitOfMeasure: string;
  unitCost: number;
  weightKg: number | null;
  isActive: boolean;
}

export interface UpdateProductDto {
  name: string;
  description: string | null;
  unitOfMeasure: string;
  unitCost: number;
  weightKg: number | null;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class ProductApiService {
  private productsUrl = `${environment.apiUrl}/products`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ProductDto[]> {
    return this.http.get<ProductDto[]>(this.productsUrl);
  }

  getById(id: number): Observable<ProductDto> {
    return this.http.get<ProductDto>(`${this.productsUrl}/${id}`);
  }

  create(dto: CreateProductDto): Observable<ProductDto> {
    return this.http.post<ProductDto>(this.productsUrl, dto);
  }

  update(id: number, dto: UpdateProductDto): Observable<ProductDto> {
    return this.http.put<ProductDto>(`${this.productsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.productsUrl}/${id}`);
  }
}

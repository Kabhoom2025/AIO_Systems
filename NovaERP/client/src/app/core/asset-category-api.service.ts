import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AssetCategoryDto {
  id: number;
  name: string;
  code: string;
  isActive: boolean;
}

export interface CreateAssetCategoryDto {
  name: string;
  code: string;
  isActive: boolean;
}

export interface UpdateAssetCategoryDto {
  name: string;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class AssetCategoryApiService {
  private categoriesUrl = `${environment.apiUrl}/asset-categories`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<AssetCategoryDto[]> {
    return this.http.get<AssetCategoryDto[]>(this.categoriesUrl);
  }

  getById(id: number): Observable<AssetCategoryDto> {
    return this.http.get<AssetCategoryDto>(`${this.categoriesUrl}/${id}`);
  }

  create(dto: CreateAssetCategoryDto): Observable<AssetCategoryDto> {
    return this.http.post<AssetCategoryDto>(this.categoriesUrl, dto);
  }

  update(id: number, dto: UpdateAssetCategoryDto): Observable<AssetCategoryDto> {
    return this.http.put<AssetCategoryDto>(`${this.categoriesUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.categoriesUrl}/${id}`);
  }
}

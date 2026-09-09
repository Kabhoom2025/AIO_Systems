import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface BranchDto {
  id: number;
  name: string;
  code: string;
  address?: string | null;
  city?: string | null;
  state?: string | null;
  country?: string | null;
  phone?: string | null;
  email?: string | null;
  timezone: string;
  isHeadOffice: boolean;
  isActive: boolean;
}

export interface CreateBranchDto {
  name: string;
  code: string;
  address?: string | null;
  city?: string | null;
  state?: string | null;
  country?: string | null;
  phone?: string | null;
  email?: string | null;
  timezone: string;
  isHeadOffice: boolean;
}

export interface UpdateBranchDto extends CreateBranchDto {
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class BranchApiService {
  private branchesUrl = `${environment.apiUrl}/branches`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<BranchDto[]> {
    return this.http.get<BranchDto[]>(this.branchesUrl);
  }

  getById(id: number): Observable<BranchDto> {
    return this.http.get<BranchDto>(`${this.branchesUrl}/${id}`);
  }

  create(dto: CreateBranchDto): Observable<BranchDto> {
    return this.http.post<BranchDto>(this.branchesUrl, dto);
  }

  update(id: number, dto: UpdateBranchDto): Observable<BranchDto> {
    return this.http.put<BranchDto>(`${this.branchesUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.branchesUrl}/${id}`);
  }
}

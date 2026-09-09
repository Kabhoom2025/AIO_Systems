import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface UserDto {
  id: number;
  name: string;
  email: string;
  roleId: number;
  roleName: string;
  branchId?: number | null;
  branchName?: string | null;
  isActive: boolean;
  lastLoginAt?: string | null;
  createdDate: string;
  photoUrl?: string | null;
}

export interface CreateUserDto {
  name: string;
  email: string;
  password: string;
  roleId: number;
  branchId?: number | null;
  photoData?: string | null;
  photoContentType?: string | null;
}

export interface UpdateUserDto {
  name: string;
  roleId: number;
  branchId?: number | null;
  isActive: boolean;
  photoData?: string | null;
  photoContentType?: string | null;
  removePhoto?: boolean;
}

export interface ResetUserPasswordDto {
  newPassword: string;
}

export interface UserRecentOrderDto {
  id: number;
  orderNumber: string;
  accountName: string;
  status: string;
  orderDate: string;
  grandTotal: number;
}

export interface UserSalesSummaryDto {
  totalOrders: number;
  draftOrders: number;
  confirmedOrders: number;
  cancelledOrders: number;
  totalSalesValue: number;
  averageOrderValue: number;
  recentOrders: UserRecentOrderDto[];
}

@Injectable({ providedIn: 'root' })
export class UserApiService {
  private usersUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(this.usersUrl);
  }

  getById(id: number): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.usersUrl}/${id}`);
  }

  create(dto: CreateUserDto): Observable<UserDto> {
    return this.http.post<UserDto>(this.usersUrl, dto);
  }

  update(id: number, dto: UpdateUserDto): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.usersUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.usersUrl}/${id}`);
  }

  resetPassword(id: number, dto: ResetUserPasswordDto): Observable<void> {
    return this.http.post<void>(`${this.usersUrl}/${id}/reset-password`, dto);
  }

  getSalesSummary(id: number): Observable<UserSalesSummaryDto> {
    return this.http.get<UserSalesSummaryDto>(`${this.usersUrl}/${id}/sales-summary`);
  }
}

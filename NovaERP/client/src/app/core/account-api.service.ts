import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AccountDto {
  id: number;
  name: string;
  industry: string | null;
  website: string | null;
  phone: string | null;
  ownerId: number;
  ownerName: string;
}

export interface CreateAccountDto {
  name: string;
  industry: string | null;
  website: string | null;
  phone: string | null;
  ownerId: number;
}

export interface UpdateAccountDto {
  name: string;
  industry: string | null;
  website: string | null;
  phone: string | null;
  ownerId: number;
}

@Injectable({ providedIn: 'root' })
export class AccountApiService {
  private accountsUrl = `${environment.apiUrl}/accounts`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<AccountDto[]> {
    return this.http.get<AccountDto[]>(this.accountsUrl);
  }

  getById(id: number): Observable<AccountDto> {
    return this.http.get<AccountDto>(`${this.accountsUrl}/${id}`);
  }

  create(dto: CreateAccountDto): Observable<AccountDto> {
    return this.http.post<AccountDto>(this.accountsUrl, dto);
  }

  update(id: number, dto: UpdateAccountDto): Observable<AccountDto> {
    return this.http.put<AccountDto>(`${this.accountsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.accountsUrl}/${id}`);
  }
}

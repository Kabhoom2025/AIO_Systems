import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ContactDto {
  id: number;
  accountId: number | null;
  accountName: string | null;
  firstName: string;
  lastName: string;
  email: string | null;
  phone: string | null;
  title: string | null;
  ownerId: number;
  ownerName: string;
}

export interface CreateContactDto {
  accountId: number | null;
  firstName: string;
  lastName: string;
  email: string | null;
  phone: string | null;
  title: string | null;
  ownerId: number;
}

export interface UpdateContactDto {
  accountId: number | null;
  firstName: string;
  lastName: string;
  email: string | null;
  phone: string | null;
  title: string | null;
  ownerId: number;
}

@Injectable({ providedIn: 'root' })
export class ContactApiService {
  private contactsUrl = `${environment.apiUrl}/contacts`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ContactDto[]> {
    return this.http.get<ContactDto[]>(this.contactsUrl);
  }

  getById(id: number): Observable<ContactDto> {
    return this.http.get<ContactDto>(`${this.contactsUrl}/${id}`);
  }

  create(dto: CreateContactDto): Observable<ContactDto> {
    return this.http.post<ContactDto>(this.contactsUrl, dto);
  }

  update(id: number, dto: UpdateContactDto): Observable<ContactDto> {
    return this.http.put<ContactDto>(`${this.contactsUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.contactsUrl}/${id}`);
  }
}

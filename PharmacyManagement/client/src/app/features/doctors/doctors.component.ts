import { Component, signal, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken } from '../../core/auth.helper';
import { HasPermissionDirective } from '../../core/permission.directive';

interface Doctor {
  id: number; name: string; registrationNumber?: string; specialty?: string;
  phone?: string; email?: string; hospitalName?: string; isActive: boolean;
}

@Component({
  selector: 'app-doctors',
  standalone: true,
  imports: [FormsModule, HasPermissionDirective],
  templateUrl: './doctors.component.html',
  styleUrl: './doctors.component.scss'
})
export class DoctorsComponent implements OnInit {
  doctors  = signal<Doctor[]>([]);
  loading  = signal(false);
  showForm = signal(false);
  editId   = signal<number | null>(null);

  orgId = getOrgIdFromToken();

  form = { name: '', registrationNumber: '', specialty: '', phone: '', email: '', hospitalName: '', isActive: true };

  constructor(private http: HttpClient) {}

  ngOnInit() { this.loadDoctors(); }

  private authHeaders() {
    return { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` };
  }

  loadDoctors() {
    this.loading.set(true);
    this.http.get<Doctor[]>(`${environment.apiUrl}/doctors/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe({
      next: data => { this.doctors.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openAdd() {
    this.editId.set(null);
    this.form = { name: '', registrationNumber: '', specialty: '', phone: '', email: '', hospitalName: '', isActive: true };
    this.showForm.set(true);
  }

  openEdit(d: Doctor) {
    this.editId.set(d.id);
    this.form = { name: d.name, registrationNumber: d.registrationNumber ?? '', specialty: d.specialty ?? '',
      phone: d.phone ?? '', email: d.email ?? '', hospitalName: d.hospitalName ?? '', isActive: d.isActive };
    this.showForm.set(true);
  }

  save() {
    const id = this.editId();
    const req$ = id
      ? this.http.put(`${environment.apiUrl}/doctors/${id}`, this.form, { headers: this.authHeaders() })
      : this.http.post(`${environment.apiUrl}/doctors/org/${this.orgId}`, this.form, { headers: this.authHeaders() });
    req$.subscribe(() => { this.showForm.set(false); this.loadDoctors(); });
  }

  delete(id: number) {
    if (!confirm('Delete this doctor?')) return;
    this.http.delete(`${environment.apiUrl}/doctors/${id}`, { headers: this.authHeaders() })
      .subscribe(() => this.loadDoctors());
  }
}

import { Component, signal, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken } from '../../core/auth.helper';
import { HasPermissionDirective } from '../../core/permission.directive';

interface Patient {
  id: number; name: string; phone?: string; email?: string; dateOfBirth?: string;
  gender?: string; address?: string; bloodGroup?: string; allergies?: string;
  chronicDiseases?: string; emergencyContactName?: string; emergencyContactPhone?: string;
  insuranceProvider?: string; insurancePolicyNumber?: string; isActive: boolean;
}

const EMPTY_FORM = {
  name: '', phone: '', email: '', dateOfBirth: '', gender: '', address: '', bloodGroup: '',
  allergies: '', chronicDiseases: '', emergencyContactName: '', emergencyContactPhone: '',
  insuranceProvider: '', insurancePolicyNumber: '', isActive: true
};

@Component({
  selector: 'app-patients',
  standalone: true,
  imports: [FormsModule, HasPermissionDirective],
  templateUrl: './patients.component.html',
  styleUrl: './patients.component.scss'
})
export class PatientsComponent implements OnInit {
  patients = signal<Patient[]>([]);
  loading  = signal(false);
  showForm = signal(false);
  editId   = signal<number | null>(null);

  orgId = getOrgIdFromToken();
  genderOptions = ['Male', 'Female', 'Other'];

  form = { ...EMPTY_FORM };

  constructor(private http: HttpClient) {}

  ngOnInit() { this.loadPatients(); }

  private authHeaders() {
    return { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` };
  }

  loadPatients() {
    this.loading.set(true);
    this.http.get<Patient[]>(`${environment.apiUrl}/patients/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe({
      next: data => { this.patients.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openAdd() {
    this.editId.set(null);
    this.form = { ...EMPTY_FORM };
    this.showForm.set(true);
  }

  openEdit(p: Patient) {
    this.editId.set(p.id);
    this.form = {
      name: p.name, phone: p.phone ?? '', email: p.email ?? '',
      dateOfBirth: p.dateOfBirth ? p.dateOfBirth.substring(0, 10) : '',
      gender: p.gender ?? '', address: p.address ?? '', bloodGroup: p.bloodGroup ?? '',
      allergies: p.allergies ?? '', chronicDiseases: p.chronicDiseases ?? '',
      emergencyContactName: p.emergencyContactName ?? '', emergencyContactPhone: p.emergencyContactPhone ?? '',
      insuranceProvider: p.insuranceProvider ?? '', insurancePolicyNumber: p.insurancePolicyNumber ?? '',
      isActive: p.isActive
    };
    this.showForm.set(true);
  }

  save() {
    const dto = { ...this.form, dateOfBirth: this.form.dateOfBirth || null };
    const id = this.editId();
    const req$ = id
      ? this.http.put(`${environment.apiUrl}/patients/${id}`, dto, { headers: this.authHeaders() })
      : this.http.post(`${environment.apiUrl}/patients/org/${this.orgId}`, dto, { headers: this.authHeaders() });
    req$.subscribe(() => { this.showForm.set(false); this.loadPatients(); });
  }

  delete(id: number) {
    if (!confirm('Delete this patient?')) return;
    this.http.delete(`${environment.apiUrl}/patients/${id}`, { headers: this.authHeaders() })
      .subscribe(() => this.loadPatients());
  }
}

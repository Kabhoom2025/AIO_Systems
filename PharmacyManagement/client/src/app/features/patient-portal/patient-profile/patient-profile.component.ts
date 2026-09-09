import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { PatientAuthService } from '../../../core/patient-auth.service';

interface PatientProfile {
  id: number;
  name: string;
  phone?: string;
  email?: string;
  dateOfBirth?: string;
  gender?: string;
  address?: string;
  bloodGroup?: string;
  allergies?: string;
  chronicDiseases?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  insuranceProvider?: string;
  insurancePolicyNumber?: string;
  isVerified: boolean;
}

@Component({
  selector: 'app-patient-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './patient-profile.component.html',
  styleUrl: './patient-profile.component.scss'
})
export class PatientProfileComponent implements OnInit {
  loading = signal(false);
  saving = signal(false);
  successMessage = signal('');
  errorMessage = signal('');
  isVerified = signal(false);
  phone = '';

  form = {
    name: '',
    email: '',
    dateOfBirth: '',
    gender: '',
    address: '',
    bloodGroup: '',
    allergies: '',
    chronicDiseases: '',
    emergencyContactName: '',
    emergencyContactPhone: '',
    insuranceProvider: '',
    insurancePolicyNumber: ''
  };

  constructor(private http: HttpClient, private auth: PatientAuthService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.http.get<PatientProfile>(`${environment.apiUrl}/patients/me`, {
      headers: this.auth.getAuthHeaders()
    }).subscribe({
      next: p => {
        this.phone = p.phone ?? '';
        this.isVerified.set(p.isVerified);
        this.form = {
          name: p.name ?? '',
          email: p.email ?? '',
          dateOfBirth: p.dateOfBirth ? p.dateOfBirth.substring(0, 10) : '',
          gender: p.gender ?? '',
          address: p.address ?? '',
          bloodGroup: p.bloodGroup ?? '',
          allergies: p.allergies ?? '',
          chronicDiseases: p.chronicDiseases ?? '',
          emergencyContactName: p.emergencyContactName ?? '',
          emergencyContactPhone: p.emergencyContactPhone ?? '',
          insuranceProvider: p.insuranceProvider ?? '',
          insurancePolicyNumber: p.insurancePolicyNumber ?? ''
        };
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  save() {
    if (!this.form.name.trim()) {
      this.errorMessage.set('Name is required.');
      return;
    }
    this.errorMessage.set('');
    this.successMessage.set('');
    this.saving.set(true);

    const dto = {
      ...this.form,
      dateOfBirth: this.form.dateOfBirth || null
    };

    this.http.put(`${environment.apiUrl}/patients/me`, dto, {
      headers: this.auth.getAuthHeaders()
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.successMessage.set('Profile updated.');
      },
      error: () => {
        this.saving.set(false);
        this.errorMessage.set('Could not save your profile. Please try again.');
      }
    });
  }
}

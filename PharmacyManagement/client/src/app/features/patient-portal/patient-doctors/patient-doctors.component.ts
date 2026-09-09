import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../../environments/environment';
import { PatientAuthService } from '../../../core/patient-auth.service';

interface PublicDoctor {
  id: number;
  name: string;
  specialty?: string;
  hospitalName?: string;
}
interface PublicBranch {
  id: number;
  name: string;
}

function todayLocal(): string {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

@Component({
  selector: 'app-patient-doctors',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './patient-doctors.component.html',
  styleUrl: './patient-doctors.component.scss'
})
export class PatientDoctorsComponent implements OnInit {
  doctors = signal<PublicDoctor[]>([]);
  branches = signal<PublicBranch[]>([]);
  loading = signal(false);
  searchTerm = '';

  bookingFor = signal<PublicDoctor | null>(null);
  form = { branchId: null as number | null, preferredDate: todayLocal(), timeSlot: 'Morning', notes: '' };
  submitting = signal(false);
  formError = signal('');
  successMessage = signal('');

  constructor(private http: HttpClient, private auth: PatientAuthService, private router: Router) {}

  ngOnInit() {
    const orgId = this.auth.getUser()?.organizationId;
    if (!orgId) return;

    this.loading.set(true);
    this.http.get<PublicDoctor[]>(`${environment.apiUrl}/doctors/public/org/${orgId}`, {
      headers: this.auth.getAuthHeaders()
    }).subscribe({
      next: data => { this.doctors.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });

    this.http.get<PublicBranch[]>(`${environment.apiUrl}/branches/public/org/${orgId}`, {
      headers: this.auth.getAuthHeaders()
    }).subscribe(data => this.branches.set(data));
  }

  get filteredDoctors(): PublicDoctor[] {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return this.doctors();
    return this.doctors().filter(d =>
      d.name.toLowerCase().includes(term) || (d.specialty ?? '').toLowerCase().includes(term)
    );
  }

  openBooking(doctor: PublicDoctor) {
    this.bookingFor.set(doctor);
    this.form = { branchId: this.branches()[0]?.id ?? null, preferredDate: todayLocal(), timeSlot: 'Morning', notes: '' };
    this.formError.set('');
    this.successMessage.set('');
  }

  closeBooking() {
    this.bookingFor.set(null);
  }

  submitBooking() {
    const doctor = this.bookingFor();
    if (!doctor) return;
    if (!this.form.branchId) {
      this.formError.set('Please select a location.');
      return;
    }
    this.formError.set('');
    this.submitting.set(true);

    this.http.post(`${environment.apiUrl}/appointments`, {
      doctorId: doctor.id,
      branchId: this.form.branchId,
      preferredDate: this.form.preferredDate,
      timeSlot: this.form.timeSlot,
      notes: this.form.notes || null
    }, { headers: this.auth.getAuthHeaders() }).subscribe({
      next: () => {
        this.submitting.set(false);
        this.successMessage.set(`Appointment requested with ${doctor.name}.`);
        setTimeout(() => {
          this.closeBooking();
          this.router.navigateByUrl('/patient/appointments');
        }, 1200);
      },
      error: () => {
        this.submitting.set(false);
        this.formError.set('Could not book the appointment. Please try again.');
      }
    });
  }
}

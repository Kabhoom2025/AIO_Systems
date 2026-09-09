import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { PatientAuthService } from '../../../core/patient-auth.service';

interface Appointment {
  id: number;
  doctorName: string;
  doctorSpecialty?: string;
  branchName: string;
  preferredDate: string;
  timeSlot: string;
  status: string;
  notes?: string;
}

const CANCELLABLE = new Set(['Requested', 'Confirmed']);

@Component({
  selector: 'app-patient-appointments',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './patient-appointments.component.html',
  styleUrl: './patient-appointments.component.scss'
})
export class PatientAppointmentsComponent implements OnInit {
  appointments = signal<Appointment[]>([]);
  loading = signal(false);
  cancellingId = signal<number | null>(null);

  constructor(private http: HttpClient, private auth: PatientAuthService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.http.get<Appointment[]>(`${environment.apiUrl}/appointments/my`, {
      headers: this.auth.getAuthHeaders()
    }).subscribe({
      next: data => { this.appointments.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  canCancel(status: string): boolean {
    return CANCELLABLE.has(status);
  }

  cancel(appt: Appointment) {
    if (!confirm(`Cancel your appointment with ${appt.doctorName}?`)) return;
    this.cancellingId.set(appt.id);
    this.http.post(`${environment.apiUrl}/appointments/${appt.id}/cancel`, {}, {
      headers: this.auth.getAuthHeaders()
    }).subscribe({
      next: () => { this.cancellingId.set(null); this.load(); },
      error: () => this.cancellingId.set(null)
    });
  }

  statusColor(status: string): string {
    return ({ Requested: '#E98A2E', Confirmed: '#1E88E5', Completed: '#2E7D4F', Cancelled: '#B23A3A' } as any)[status] ?? '#4B5D58';
  }
}

import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken } from '../../core/auth.helper';
import { HasPermissionDirective } from '../../core/permission.directive';

interface Appointment {
  id: number;
  patientId: number;
  patientName: string;
  patientPhone?: string;
  doctorId: number;
  doctorName: string;
  doctorSpecialty?: string;
  branchId: number;
  branchName: string;
  preferredDate: string;
  timeSlot: string;
  status: string;
  notes?: string;
}

const NEXT_STATUS: Record<string, string> = { Requested: 'Confirmed', Confirmed: 'Completed' };
const NEXT_LABEL: Record<string, string> = { Requested: 'Confirm', Confirmed: 'Complete' };
const TERMINAL = new Set(['Completed', 'Cancelled']);

@Component({
  selector: 'app-appointments',
  standalone: true,
  imports: [CommonModule, FormsModule, HasPermissionDirective],
  templateUrl: './appointments.component.html',
  styleUrl: './appointments.component.scss'
})
export class AppointmentsComponent implements OnInit {
  appointments = signal<Appointment[]>([]);
  loading = signal(false);
  statusFilter = 'All';
  searchTerm = '';
  updatingId = signal<number | null>(null);

  statusOptions = ['All', 'Requested', 'Confirmed', 'Completed', 'Cancelled'];
  orgId = getOrgIdFromToken();

  constructor(private http: HttpClient) {}

  ngOnInit() { this.load(); }

  private authHeaders() {
    return { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` };
  }

  load() {
    this.loading.set(true);
    this.http.get<Appointment[]>(`${environment.apiUrl}/appointments/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe({
      next: data => { this.appointments.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  get filteredAppointments(): Appointment[] {
    const term = this.searchTerm.trim().toLowerCase();
    return this.appointments().filter(a => {
      const matchesStatus = this.statusFilter === 'All' || a.status === this.statusFilter;
      const matchesTerm = !term ||
        a.patientName.toLowerCase().includes(term) ||
        a.doctorName.toLowerCase().includes(term);
      return matchesStatus && matchesTerm;
    });
  }

  nextAction(status: string): string | null {
    return NEXT_LABEL[status] ?? null;
  }

  canCancel(status: string): boolean {
    return !TERMINAL.has(status);
  }

  advance(a: Appointment) {
    const next = NEXT_STATUS[a.status];
    if (!next) return;
    this.updateStatus(a, next);
  }

  cancel(a: Appointment) {
    if (!confirm(`Cancel the appointment for ${a.patientName} with ${a.doctorName}?`)) return;
    this.updateStatus(a, 'Cancelled');
  }

  private updateStatus(a: Appointment, status: string) {
    this.updatingId.set(a.id);
    this.http.post(`${environment.apiUrl}/appointments/${a.id}/status`, { status }, {
      headers: this.authHeaders()
    }).subscribe({
      next: () => { this.updatingId.set(null); this.load(); },
      error: () => this.updatingId.set(null)
    });
  }

  statusColor(status: string): string {
    return ({ Requested: '#E98A2E', Confirmed: '#1E88E5', Completed: '#2E7D4F', Cancelled: '#B23A3A' } as any)[status] ?? '#555';
  }
}

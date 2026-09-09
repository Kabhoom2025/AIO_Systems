import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { environment } from '../../../environments/environment';

interface PublicOrganization { id: number; name: string; phone?: string; }
interface PublicDoctor { id: number; name: string; specialty?: string; hospitalName?: string; }
interface PublicBranch { id: number; name: string; address?: string; phone?: string; email?: string; }

const AVATAR_GRADIENTS = [
  'linear-gradient(135deg,#0E4D42,#1B6F60)',
  'linear-gradient(135deg,#C96F1B,#E98A2E)',
  'linear-gradient(135deg,#0E4D42,#2C9481)',
  'linear-gradient(135deg,#B98A22,#E98A2E)'
];

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss'
})
export class LandingComponent implements OnInit {
  orgName = signal('Amara Hospitals');
  orgPhone = signal('');
  doctors = signal<PublicDoctor[]>([]);
  branches = signal<PublicBranch[]>([]);
  doctorsLoading = signal(true);
  branchesLoading = signal(true);
  activeBookingTab = signal<'doctor' | 'lab' | 'checkup'>('doctor');

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.http.get<PublicOrganization>(`${environment.apiUrl}/organizations/public/default`).subscribe({
      next: org => {
        this.orgName.set(org.name);
        this.orgPhone.set(org.phone || '1800-419-0000');

        this.http.get<PublicDoctor[]>(`${environment.apiUrl}/doctors/public/org/${org.id}`).subscribe({
          next: data => { this.doctors.set(data.slice(0, 4)); this.doctorsLoading.set(false); },
          error: () => this.doctorsLoading.set(false)
        });

        this.http.get<PublicBranch[]>(`${environment.apiUrl}/branches/public/org/${org.id}`).subscribe({
          next: data => { this.branches.set(data.slice(0, 4)); this.branchesLoading.set(false); },
          error: () => this.branchesLoading.set(false)
        });
      },
      error: () => { this.doctorsLoading.set(false); this.branchesLoading.set(false); }
    });
  }

  avatarGradient(index: number): string {
    return AVATAR_GRADIENTS[index % AVATAR_GRADIENTS.length];
  }

  initials(name: string): string {
    return name.replace(/^Dr\.?\s*/i, '').split(' ').map(p => p.charAt(0)).slice(0, 2).join('').toUpperCase();
  }

  setBookingTab(tab: 'doctor' | 'lab' | 'checkup') {
    this.activeBookingTab.set(tab);
  }

  telHref(): string {
    return `tel:${this.orgPhone().replace(/[^0-9+]/g, '')}`;
  }
}

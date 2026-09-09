import { Component, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { PatientAuthService } from '../../../core/patient-auth.service';

interface PublicBranch {
  id: number;
  name: string;
  address?: string;
  phone?: string;
  email?: string;
}

@Component({
  selector: 'app-patient-locations',
  standalone: true,
  imports: [],
  templateUrl: './patient-locations.component.html',
  styleUrl: './patient-locations.component.scss'
})
export class PatientLocationsComponent implements OnInit {
  locations = signal<PublicBranch[]>([]);
  loading = signal(false);

  constructor(private http: HttpClient, private auth: PatientAuthService) {}

  ngOnInit() {
    const orgId = this.auth.getUser()?.organizationId;
    if (!orgId) return;

    this.loading.set(true);
    this.http.get<PublicBranch[]>(`${environment.apiUrl}/branches/public/org/${orgId}`, {
      headers: this.auth.getAuthHeaders()
    }).subscribe({
      next: data => { this.locations.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  mapsUrl(address?: string): string {
    return `https://maps.google.com/?q=${encodeURIComponent(address ?? '')}`;
  }
}

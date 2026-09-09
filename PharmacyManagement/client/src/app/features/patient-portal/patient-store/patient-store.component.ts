import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../../environments/environment';
import { PatientAuthService } from '../../../core/patient-auth.service';
import { PatientCartService } from '../../../core/patient-cart.service';

interface PublicMedicine {
  id: number;
  name: string;
  genericName?: string;
  category?: string;
  mrp: number;
  unit: string;
  packSize?: string;
  totalStock: number;
}

@Component({
  selector: 'app-patient-store',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './patient-store.component.html',
  styleUrl: './patient-store.component.scss'
})
export class PatientStoreComponent implements OnInit {
  medicines = signal<PublicMedicine[]>([]);
  loading = signal(false);
  searchTerm = '';
  quantities: Record<number, number> = {};
  addedId = signal<number | null>(null);

  constructor(
    private http: HttpClient,
    private auth: PatientAuthService,
    public cart: PatientCartService,
    private router: Router
  ) {}

  ngOnInit() {
    const orgId = this.auth.getUser()?.organizationId;
    if (!orgId) return;

    this.loading.set(true);
    this.http.get<PublicMedicine[]>(`${environment.apiUrl}/medicines/public/org/${orgId}`, {
      headers: this.auth.getAuthHeaders()
    }).subscribe({
      next: data => {
        this.medicines.set(data);
        data.forEach(m => this.quantities[m.id] = 1);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  get filteredMedicines(): PublicMedicine[] {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return this.medicines();
    return this.medicines().filter(m =>
      m.name.toLowerCase().includes(term) || (m.genericName ?? '').toLowerCase().includes(term)
    );
  }

  addToCart(m: PublicMedicine) {
    const qty = Math.max(1, Math.min(this.quantities[m.id] ?? 1, m.totalStock));
    this.cart.add({ medicineId: m.id, name: m.name, mrp: m.mrp, unit: m.unit }, qty);
    this.addedId.set(m.id);
    setTimeout(() => this.addedId.set(null), 1000);
  }

  goToCart() {
    this.router.navigateByUrl('/patient/cart');
  }
}

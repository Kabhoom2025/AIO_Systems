import { Component, signal, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken } from '../../core/auth.helper';
import { HasPermissionDirective } from '../../core/permission.directive';
import { exportToCsv } from '../../core/csv.util';

interface Supplier {
  id: number; name: string; contactPerson?: string; phone?: string; email?: string;
  address?: string; gstNumber?: string; paymentTerms?: string; creditLimit: number; isActive: boolean;
}

@Component({
  selector: 'app-suppliers',
  standalone: true,
  imports: [FormsModule, HasPermissionDirective],
  templateUrl: './suppliers.component.html',
  styleUrl: './suppliers.component.scss'
})
export class SuppliersComponent implements OnInit {
  suppliers = signal<Supplier[]>([]);
  loading   = signal(false);
  showForm  = signal(false);
  editId    = signal<number | null>(null);

  orgId = getOrgIdFromToken();

  form = {
    name: '', contactPerson: '', phone: '', email: '',
    address: '', gstNumber: '', paymentTerms: '', creditLimit: 0, isActive: true
  };

  constructor(private http: HttpClient) {}

  ngOnInit() { this.loadSuppliers(); }

  loadSuppliers() {
    this.loading.set(true);
    this.http.get<Supplier[]>(`${environment.apiUrl}/suppliers/org/${this.orgId}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` }
    }).subscribe({
      next: data => { this.suppliers.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openAdd() {
    this.editId.set(null);
    this.form = { name: '', contactPerson: '', phone: '', email: '',
      address: '', gstNumber: '', paymentTerms: '', creditLimit: 0, isActive: true };
    this.showForm.set(true);
  }

  openEdit(s: Supplier) {
    this.editId.set(s.id);
    this.form = { name: s.name, contactPerson: s.contactPerson ?? '', phone: s.phone ?? '',
      email: s.email ?? '', address: s.address ?? '', gstNumber: s.gstNumber ?? '',
      paymentTerms: s.paymentTerms ?? '', creditLimit: s.creditLimit, isActive: s.isActive };
    this.showForm.set(true);
  }

  save() {
    const token = localStorage.getItem(environment.tokenKey);
    const headers = { Authorization: `Bearer ${token}` };
    const id = this.editId();
    const req$ = id
      ? this.http.put(`${environment.apiUrl}/suppliers/${id}`, this.form, { headers })
      : this.http.post(`${environment.apiUrl}/suppliers/org/${this.orgId}`, this.form, { headers });
    req$.subscribe(() => { this.showForm.set(false); this.loadSuppliers(); });
  }

  delete(id: number) {
    if (!confirm('Delete this supplier?')) return;
    this.http.delete(`${environment.apiUrl}/suppliers/${id}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` }
    }).subscribe(() => this.loadSuppliers());
  }

  exportCsv() {
    exportToCsv('suppliers.csv', this.suppliers().map(s => ({
      name: s.name, contactPerson: s.contactPerson ?? '', phone: s.phone ?? '',
      email: s.email ?? '', gstNumber: s.gstNumber ?? '', isActive: s.isActive
    })));
  }
}

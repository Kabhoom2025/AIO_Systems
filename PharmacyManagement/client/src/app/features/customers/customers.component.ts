import { Component, signal, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken } from '../../core/auth.helper';
import { HasPermissionDirective } from '../../core/permission.directive';

interface Customer {
  id: number; name: string; phone?: string; email?: string; address?: string;
  membershipLevel: string; loyaltyPoints: number; walletBalance: number; isActive: boolean;
}

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [FormsModule, HasPermissionDirective],
  templateUrl: './customers.component.html',
  styleUrl: './customers.component.scss'
})
export class CustomersComponent implements OnInit {
  customers = signal<Customer[]>([]);
  loading   = signal(false);
  showForm  = signal(false);
  editId    = signal<number | null>(null);

  orgId = getOrgIdFromToken();
  membershipOptions = ['Bronze', 'Silver', 'Gold', 'Platinum'];

  form = {
    name: '', phone: '', email: '', address: '', membershipLevel: 'Bronze',
    loyaltyPoints: 0, walletBalance: 0, isActive: true
  };

  constructor(private http: HttpClient) {}

  ngOnInit() { this.loadCustomers(); }

  private authHeaders() {
    return { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` };
  }

  loadCustomers() {
    this.loading.set(true);
    this.http.get<Customer[]>(`${environment.apiUrl}/customers/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe({
      next: data => { this.customers.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openAdd() {
    this.editId.set(null);
    this.form = { name: '', phone: '', email: '', address: '', membershipLevel: 'Bronze',
      loyaltyPoints: 0, walletBalance: 0, isActive: true };
    this.showForm.set(true);
  }

  openEdit(c: Customer) {
    this.editId.set(c.id);
    this.form = { name: c.name, phone: c.phone ?? '', email: c.email ?? '', address: c.address ?? '',
      membershipLevel: c.membershipLevel, loyaltyPoints: c.loyaltyPoints,
      walletBalance: c.walletBalance, isActive: c.isActive };
    this.showForm.set(true);
  }

  save() {
    const id = this.editId();
    const req$ = id
      ? this.http.put(`${environment.apiUrl}/customers/${id}`, this.form, { headers: this.authHeaders() })
      : this.http.post(`${environment.apiUrl}/customers/org/${this.orgId}`, this.form, { headers: this.authHeaders() });
    req$.subscribe(() => { this.showForm.set(false); this.loadCustomers(); });
  }

  delete(id: number) {
    if (!confirm('Delete this customer?')) return;
    this.http.delete(`${environment.apiUrl}/customers/${id}`, { headers: this.authHeaders() })
      .subscribe(() => this.loadCustomers());
  }

  membershipColor(level: string) {
    return ({ Bronze: '#8d6e63', Silver: '#757575', Gold: '#f9a825', Platinum: '#5e35b1' } as any)[level] ?? '#757575';
  }
}

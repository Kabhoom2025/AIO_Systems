import { Component, signal, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken } from '../../core/auth.helper';
import { HasPermissionDirective } from '../../core/permission.directive';

interface Branch {
  id: number; name: string; code: string; type: string;
  address?: string; phone?: string; email?: string; isActive: boolean;
}

@Component({
  selector: 'app-branches',
  standalone: true,
  imports: [FormsModule, HasPermissionDirective],
  templateUrl: './branches.component.html',
  styleUrl: './branches.component.scss'
})
export class BranchesComponent implements OnInit {
  branches = signal<Branch[]>([]);
  loading  = signal(false);
  showForm = signal(false);
  editId   = signal<number | null>(null);

  orgId = getOrgIdFromToken();
  typeOptions = ['Retail', 'Warehouse'];

  form = { name: '', code: '', type: 'Retail', address: '', phone: '', email: '', isActive: true };

  constructor(private http: HttpClient) {}

  ngOnInit() { this.loadBranches(); }

  private authHeaders() {
    return { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` };
  }

  loadBranches() {
    this.loading.set(true);
    this.http.get<Branch[]>(`${environment.apiUrl}/branches/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe({
      next: data => { this.branches.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openAdd() {
    this.editId.set(null);
    this.form = { name: '', code: '', type: 'Retail', address: '', phone: '', email: '', isActive: true };
    this.showForm.set(true);
  }

  openEdit(b: Branch) {
    this.editId.set(b.id);
    this.form = { name: b.name, code: b.code, type: b.type, address: b.address ?? '',
      phone: b.phone ?? '', email: b.email ?? '', isActive: b.isActive };
    this.showForm.set(true);
  }

  save() {
    const id = this.editId();
    const req$ = id
      ? this.http.put(`${environment.apiUrl}/branches/${id}`, this.form, { headers: this.authHeaders() })
      : this.http.post(`${environment.apiUrl}/branches/org/${this.orgId}`, this.form, { headers: this.authHeaders() });
    req$.subscribe(() => { this.showForm.set(false); this.loadBranches(); });
  }

  delete(id: number) {
    if (!confirm('Delete this branch?')) return;
    this.http.delete(`${environment.apiUrl}/branches/${id}`, { headers: this.authHeaders() })
      .subscribe(() => this.loadBranches());
  }
}

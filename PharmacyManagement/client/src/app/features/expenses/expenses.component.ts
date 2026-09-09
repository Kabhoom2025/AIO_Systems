import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken, getBranchIdFromToken } from '../../core/auth.helper';
import { HasPermissionDirective } from '../../core/permission.directive';
import { exportToCsv } from '../../core/csv.util';

interface Expense {
  id: number; branchId: number; branchName: string; category: string;
  amount: number; expenseDate: string; paymentMethod: string; notes?: string;
}
interface Branch { id: number; name: string; }

function todayLocal(): string {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

@Component({
  selector: 'app-expenses',
  standalone: true,
  imports: [CommonModule, FormsModule, HasPermissionDirective],
  templateUrl: './expenses.component.html',
  styleUrl: './expenses.component.scss'
})
export class ExpensesComponent implements OnInit {
  expenses = signal<Expense[]>([]);
  branches = signal<Branch[]>([]);
  loading  = signal(false);
  showForm = signal(false);
  editId   = signal<number | null>(null);

  orgId = getOrgIdFromToken();
  categoryOptions = ['Rent', 'Utilities', 'Salaries', 'Marketing', 'Maintenance', 'Other'];
  paymentOptions = ['Cash', 'Card', 'UPI', 'Bank Transfer'];

  form = {
    branchId: getBranchIdFromToken() ?? 0, category: 'Other', amount: 0,
    expenseDate: todayLocal(), paymentMethod: 'Cash', notes: ''
  };

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadExpenses();
    this.loadBranches();
  }

  private authHeaders() {
    return { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` };
  }

  loadExpenses() {
    this.loading.set(true);
    this.http.get<Expense[]>(`${environment.apiUrl}/expenses/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe({
      next: data => { this.expenses.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  loadBranches() {
    this.http.get<Branch[]>(`${environment.apiUrl}/branches/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe(data => this.branches.set(data));
  }

  openAdd() {
    this.editId.set(null);
    this.form = {
      branchId: getBranchIdFromToken() ?? (this.branches()[0]?.id ?? 0), category: 'Other', amount: 0,
      expenseDate: todayLocal(), paymentMethod: 'Cash', notes: ''
    };
    this.showForm.set(true);
  }

  openEdit(e: Expense) {
    this.editId.set(e.id);
    this.form = {
      branchId: e.branchId, category: e.category, amount: e.amount,
      expenseDate: e.expenseDate.substring(0, 10), paymentMethod: e.paymentMethod, notes: e.notes ?? ''
    };
    this.showForm.set(true);
  }

  save() {
    const id = this.editId();
    const req$ = id
      ? this.http.put(`${environment.apiUrl}/expenses/${id}`, this.form, { headers: this.authHeaders() })
      : this.http.post(`${environment.apiUrl}/expenses/org/${this.orgId}`, this.form, { headers: this.authHeaders() });
    req$.subscribe(() => { this.showForm.set(false); this.loadExpenses(); });
  }

  delete(id: number) {
    if (!confirm('Delete this expense?')) return;
    this.http.delete(`${environment.apiUrl}/expenses/${id}`, { headers: this.authHeaders() })
      .subscribe(() => this.loadExpenses());
  }

  exportCsv() {
    exportToCsv('expenses.csv', this.expenses().map(e => ({
      category: e.category, amount: e.amount, expenseDate: e.expenseDate,
      paymentMethod: e.paymentMethod, notes: e.notes ?? ''
    })));
  }
}

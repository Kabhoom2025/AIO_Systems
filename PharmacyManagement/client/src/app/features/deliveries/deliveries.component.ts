import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken, getBranchIdFromToken } from '../../core/auth.helper';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';

interface Sale { id: number; invoiceNumber: string; customerId?: number; customerName?: string; }
interface UserSummary { id: number; name: string; email: string; roleName: string; }

interface Delivery {
  id: number; saleId: number; saleInvoiceNumber: string; branchName: string;
  customerId?: number; customerName?: string; deliveryStaffId?: number; deliveryStaffName?: string;
  address: string; scheduledDate: string; deliveryCharge: number; status: string;
  otpCode: string; otpVerifiedDate?: string; notes?: string;
}

@Component({
  selector: 'app-deliveries',
  standalone: true,
  imports: [CommonModule, FormsModule, HasPermissionDirective],
  templateUrl: './deliveries.component.html',
  styleUrl: './deliveries.component.scss'
})
export class DeliveriesComponent implements OnInit {
  deliveries = signal<Delivery[]>([]);
  sales = signal<Sale[]>([]);
  deliveryStaff = signal<UserSummary[]>([]);
  loading = signal(false);
  showForm = signal(false);
  otpInputs: Record<number, string> = {};

  orgId = getOrgIdFromToken();
  branchId = getBranchIdFromToken();

  form = {
    saleId: null as number | null, address: '', scheduledDate: '', deliveryCharge: 0, notes: ''
  };

  constructor(private http: HttpClient, private notify: NotificationService) {}

  ngOnInit() {
    this.loadDeliveries();
    this.loadSales();
    this.loadUsers();
  }

  private authHeaders() {
    return { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` };
  }

  loadDeliveries() {
    this.loading.set(true);
    this.http.get<Delivery[]>(`${environment.apiUrl}/deliveries/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe({
      next: data => { this.deliveries.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  loadSales() {
    this.http.get<Sale[]>(`${environment.apiUrl}/sales/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe(data => this.sales.set(data));
  }

  loadUsers() {
    this.http.get<UserSummary[]>(`${environment.apiUrl}/users/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe(data => this.deliveryStaff.set(data.filter(u => u.roleName === 'Delivery Staff')));
  }

  openAdd() {
    this.form = { saleId: null, address: '', scheduledDate: '', deliveryCharge: 0, notes: '' };
    this.showForm.set(true);
  }

  save() {
    if (!this.form.saleId || !this.branchId) return;
    const sale = this.sales().find(s => s.id === this.form.saleId);
    const dto = {
      saleId: this.form.saleId,
      branchId: this.branchId,
      customerId: sale?.customerId ?? null,
      address: this.form.address,
      scheduledDate: this.form.scheduledDate,
      deliveryCharge: this.form.deliveryCharge,
      notes: this.form.notes
    };
    this.http.post(`${environment.apiUrl}/deliveries/org/${this.orgId}`, dto, {
      headers: this.authHeaders()
    }).subscribe({
      next: () => { this.showForm.set(false); this.loadDeliveries(); this.notify.success('Delivery scheduled.'); },
      error: (err: HttpErrorResponse) => this.notify.error(err.error?.message || 'Failed to schedule delivery.')
    });
  }

  assignStaff(delivery: Delivery, staffId: string) {
    if (!staffId) return;
    this.http.patch(`${environment.apiUrl}/deliveries/${delivery.id}/assign`, { deliveryStaffId: Number(staffId) }, {
      headers: this.authHeaders()
    }).subscribe({
      next: () => this.loadDeliveries(),
      error: (err: HttpErrorResponse) => this.notify.error(err.error?.message || 'Failed to assign staff.')
    });
  }

  updateStatus(delivery: Delivery, status: string) {
    this.http.patch(`${environment.apiUrl}/deliveries/${delivery.id}/status`, { status }, {
      headers: this.authHeaders()
    }).subscribe({
      next: () => this.loadDeliveries(),
      error: (err: HttpErrorResponse) => this.notify.error(err.error?.message || 'Failed to update status.')
    });
  }

  verifyOtp(delivery: Delivery) {
    const otpCode = this.otpInputs[delivery.id];
    if (!otpCode) return;
    this.http.patch(`${environment.apiUrl}/deliveries/${delivery.id}/verify-otp`, { otpCode }, {
      headers: this.authHeaders()
    }).subscribe({
      next: () => { this.loadDeliveries(); this.notify.success('Delivery confirmed.'); },
      error: (err: HttpErrorResponse) => this.notify.error(err.error?.message || 'OTP verification failed.')
    });
  }

  statusColor(status: string) {
    return ({
      Pending: '#757575', Assigned: '#1e88e5', OutForDelivery: '#fb8c00',
      Delivered: '#43a047', Cancelled: '#e53935'
    } as any)[status] ?? '#757575';
  }
}

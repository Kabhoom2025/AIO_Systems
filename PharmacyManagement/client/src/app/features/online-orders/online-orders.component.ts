import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken } from '../../core/auth.helper';
import { HasPermissionDirective } from '../../core/permission.directive';

interface OrderItem {
  id: number; medicineName: string; batchNumber: string; quantity: number;
  unitPrice: number; discountAmount: number; gstPercent: number; lineTotal: number;
}
interface OnlineOrder {
  id: number; invoiceNumber: string; saleDate: string; branchName: string;
  patientId?: number; patientName?: string; patientPhone?: string;
  subtotal: number; discountAmount: number; taxAmount: number;
  totalAmount: number; paymentMethod: string; status: string; items: OrderItem[];
}

@Component({
  selector: 'app-online-orders',
  standalone: true,
  imports: [CommonModule, HasPermissionDirective],
  templateUrl: './online-orders.component.html',
  styleUrl: './online-orders.component.scss'
})
export class OnlineOrdersComponent implements OnInit {
  orders = signal<OnlineOrder[]>([]);
  loading = signal(false);
  detail = signal<OnlineOrder | null>(null);
  fulfillingId = signal<number | null>(null);

  orgId = getOrgIdFromToken();

  constructor(private http: HttpClient) {}

  ngOnInit() { this.load(); }

  private authHeaders() {
    return { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` };
  }

  load() {
    this.loading.set(true);
    this.http.get<OnlineOrder[]>(`${environment.apiUrl}/sales/online/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe({
      next: data => { this.orders.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  viewDetail(order: OnlineOrder) { this.detail.set(order); }
  closeDetail() { this.detail.set(null); }

  fulfill(order: OnlineOrder) {
    this.fulfillingId.set(order.id);
    this.http.post(`${environment.apiUrl}/sales/${order.id}/fulfill`, {}, {
      headers: this.authHeaders()
    }).subscribe({
      next: () => { this.fulfillingId.set(null); this.load(); },
      error: () => this.fulfillingId.set(null)
    });
  }

  statusColor(status: string): string {
    return ({ Pending: '#E98A2E', Completed: '#2E7D4F' } as any)[status] ?? '#555';
  }
}

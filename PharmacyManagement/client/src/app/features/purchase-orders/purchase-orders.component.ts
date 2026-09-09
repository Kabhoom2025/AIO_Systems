import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken } from '../../core/auth.helper';
import { exportToCsv } from '../../core/csv.util';

interface PurchaseOrderItem {
  id: number; medicineId: number; medicineName: string;
  quantity: number; unitPrice: number; receivedQuantity: number;
}

interface PurchaseOrder {
  id: number; supplierId: number; supplierName: string; poNumber: string;
  orderDate: string; expectedDeliveryDate?: string; status: string;
  totalAmount: number; notes?: string; items: PurchaseOrderItem[];
}

interface Supplier { id: number; name: string; isActive: boolean; }
interface Medicine { id: number; name: string; unit: string; }

@Component({
  selector: 'app-purchase-orders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './purchase-orders.component.html',
  styleUrl: './purchase-orders.component.scss'
})
export class PurchaseOrdersComponent implements OnInit {
  purchaseOrders = signal<PurchaseOrder[]>([]);
  suppliers      = signal<Supplier[]>([]);
  medicines      = signal<Medicine[]>([]);
  loading        = signal(false);
  showForm       = signal(false);
  detail         = signal<PurchaseOrder | null>(null);

  orgId = getOrgIdFromToken();

  form: {
    supplierId: number | null;
    expectedDeliveryDate: string;
    notes: string;
    items: { medicineId: number | null; quantity: number; unitPrice: number }[];
  } = { supplierId: null, expectedDeliveryDate: '', notes: '', items: [{ medicineId: null, quantity: 1, unitPrice: 0 }] };

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadPurchaseOrders();
    this.loadSuppliers();
    this.loadMedicines();
  }

  private authHeaders() {
    return { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` };
  }

  loadPurchaseOrders() {
    this.loading.set(true);
    this.http.get<PurchaseOrder[]>(`${environment.apiUrl}/purchase-orders/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe({
      next: data => { this.purchaseOrders.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  loadSuppliers() {
    this.http.get<Supplier[]>(`${environment.apiUrl}/suppliers/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe(data => this.suppliers.set(data.filter(s => s.isActive)));
  }

  loadMedicines() {
    this.http.get<Medicine[]>(`${environment.apiUrl}/medicines/org/${this.orgId}`, {
      headers: this.authHeaders()
    }).subscribe(data => this.medicines.set(data));
  }

  openAdd() {
    this.form = { supplierId: null, expectedDeliveryDate: '', notes: '',
      items: [{ medicineId: null, quantity: 1, unitPrice: 0 }] };
    this.showForm.set(true);
  }

  addItemRow() { this.form.items.push({ medicineId: null, quantity: 1, unitPrice: 0 }); }
  removeItemRow(i: number) { this.form.items.splice(i, 1); }

  get formTotal(): number {
    return this.form.items.reduce((sum, i) => sum + (i.quantity || 0) * (i.unitPrice || 0), 0);
  }

  save() {
    this.http.post(`${environment.apiUrl}/purchase-orders/org/${this.orgId}`, this.form, {
      headers: this.authHeaders()
    }).subscribe(() => { this.showForm.set(false); this.loadPurchaseOrders(); });
  }

  updateStatus(po: PurchaseOrder, status: string) {
    this.http.patch(`${environment.apiUrl}/purchase-orders/${po.id}/status`, { status }, {
      headers: this.authHeaders()
    }).subscribe(() => this.loadPurchaseOrders());
  }

  viewDetail(po: PurchaseOrder) { this.detail.set(po); }
  closeDetail() { this.detail.set(null); }

  statusColor(s: string) {
    return ({ Draft: '#757575', Sent: '#1e88e5', PartiallyReceived: '#fb8c00', Received: '#43a047', Cancelled: '#e53935' } as any)[s] ?? '#757575';
  }

  exportCsv() {
    exportToCsv('purchase-orders.csv', this.purchaseOrders().map(po => ({
      poNumber: po.poNumber, supplierName: po.supplierName, orderDate: po.orderDate,
      status: po.status, totalAmount: po.totalAmount
    })));
  }

  printPo() {
    window.print();
  }
}

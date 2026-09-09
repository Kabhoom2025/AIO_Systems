import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken } from '../../core/auth.helper';
import { exportToCsv } from '../../core/csv.util';

interface SaleItem {
  id: number; medicineName: string; batchNumber: string; quantity: number;
  unitPrice: number; discountAmount: number; gstPercent: number; lineTotal: number;
}
interface Sale {
  id: number; invoiceNumber: string; saleDate: string; branchName: string;
  customerName?: string; subtotal: number; discountAmount: number; taxAmount: number;
  totalAmount: number; paymentMethod: string; status: string; items: SaleItem[];
}

@Component({
  selector: 'app-sales-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sales-history.component.html',
  styleUrl: './sales-history.component.scss'
})
export class SalesHistoryComponent implements OnInit {
  sales = signal<Sale[]>([]);
  loading = signal(false);
  detail = signal<Sale | null>(null);

  orgId = getOrgIdFromToken();

  constructor(private http: HttpClient) {}

  ngOnInit() { this.loadSales(); }

  loadSales() {
    this.loading.set(true);
    this.http.get<Sale[]>(`${environment.apiUrl}/sales/org/${this.orgId}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` }
    }).subscribe({
      next: data => { this.sales.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  viewDetail(sale: Sale) { this.detail.set(sale); }
  closeDetail() { this.detail.set(null); }

  exportCsv() {
    exportToCsv('sales-history.csv', this.sales().map(s => ({
      invoiceNumber: s.invoiceNumber, saleDate: s.saleDate, customerName: s.customerName ?? 'Walk-in',
      paymentMethod: s.paymentMethod, totalAmount: s.totalAmount
    })));
  }

  printInvoice() {
    window.print();
  }
}

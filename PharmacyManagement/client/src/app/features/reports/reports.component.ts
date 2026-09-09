import { Component, signal, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { Chart, registerables } from 'chart.js';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken } from '../../core/auth.helper';

Chart.register(...registerables);

interface ProfitLoss {
  revenue: number; costOfGoodsSold: number; grossProfit: number; expenses: number; netProfit: number;
}
interface GstReport { outputTax: number; inputTax: number; netGstPayable: number; }
interface CashBookEntry { date: string; description: string; type: string; amount: number; runningBalance: number; }

interface SalesTrendPoint { date: string; amount: number; }
interface PaymentMethodBreakdown { method: string; amount: number; }
interface SalesSummary {
  totalSales: number; transactionCount: number; averageTicket: number;
  dailyTrend: SalesTrendPoint[]; byPaymentMethod: PaymentMethodBreakdown[];
}
interface TopMedicine { medicineId: number; medicineName: string; quantitySold: number; revenue: number; }
interface InventoryValuationItem { medicineId: number; medicineName: string; totalStock: number; purchasePrice: number; stockValue: number; }
interface InventoryValuationReport { items: InventoryValuationItem[]; grandTotal: number; }

function toLocalDateString(d: Date): string {
  const year = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}
function firstOfMonth(): string {
  const d = new Date();
  return toLocalDateString(new Date(d.getFullYear(), d.getMonth(), 1));
}
function today(): string {
  return toLocalDateString(new Date());
}

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss'
})
export class ReportsComponent implements OnInit {
  orgId = getOrgIdFromToken();

  from = firstOfMonth();
  to = today();

  activeTab = signal<'financial' | 'operational'>('financial');

  profitLoss = signal<ProfitLoss | null>(null);
  gstReport = signal<GstReport | null>(null);
  cashBook = signal<CashBookEntry[]>([]);
  loading = signal(false);

  salesSummary = signal<SalesSummary | null>(null);
  topMedicines = signal<TopMedicine[]>([]);
  inventoryValuation = signal<InventoryValuationReport | null>(null);
  operationalLoading = signal(false);

  @ViewChild('salesChartCanvas') salesChartCanvas?: ElementRef<HTMLCanvasElement>;
  private chart?: Chart;

  constructor(private http: HttpClient) {}

  ngOnInit() { this.runReports(); }

  private authHeaders() {
    return { Authorization: `Bearer ${localStorage.getItem(environment.tokenKey)}` };
  }

  selectTab(tab: 'financial' | 'operational') {
    this.activeTab.set(tab);
    if (tab === 'operational' && !this.salesSummary()) {
      this.runOperationalReports();
    }
  }

  runReports() {
    this.loading.set(true);
    const params = `from=${this.from}&to=${this.to}`;
    const headers = this.authHeaders();

    this.http.get<ProfitLoss>(`${environment.apiUrl}/reports/profit-loss/org/${this.orgId}?${params}`, { headers })
      .subscribe(data => this.profitLoss.set(data));
    this.http.get<GstReport>(`${environment.apiUrl}/reports/gst/org/${this.orgId}?${params}`, { headers })
      .subscribe(data => this.gstReport.set(data));
    this.http.get<CashBookEntry[]>(`${environment.apiUrl}/reports/cash-book/org/${this.orgId}?${params}`, { headers })
      .subscribe({
        next: data => { this.cashBook.set(data); this.loading.set(false); },
        error: () => this.loading.set(false)
      });

    if (this.activeTab() === 'operational') {
      this.runOperationalReports();
    }
  }

  runOperationalReports() {
    this.operationalLoading.set(true);
    const params = `from=${this.from}&to=${this.to}`;
    const headers = this.authHeaders();

    this.http.get<SalesSummary>(`${environment.apiUrl}/reports/sales-summary/org/${this.orgId}?${params}`, { headers })
      .subscribe(data => {
        this.salesSummary.set(data);
        setTimeout(() => this.renderChart(data.dailyTrend), 0);
      });

    this.http.get<TopMedicine[]>(`${environment.apiUrl}/reports/top-medicines/org/${this.orgId}?${params}`, { headers })
      .subscribe(data => this.topMedicines.set(data));

    this.http.get<InventoryValuationReport>(`${environment.apiUrl}/reports/inventory-valuation/org/${this.orgId}`, { headers })
      .subscribe({
        next: data => { this.inventoryValuation.set(data); this.operationalLoading.set(false); },
        error: () => this.operationalLoading.set(false)
      });
  }

  private renderChart(trend: SalesTrendPoint[]) {
    const canvas = this.salesChartCanvas?.nativeElement;
    if (!canvas) return;

    this.chart?.destroy();
    this.chart = new Chart(canvas, {
      type: 'line',
      data: {
        labels: trend.map(p => new Date(p.date).toLocaleDateString()),
        datasets: [{
          label: 'Sales',
          data: trend.map(p => p.amount),
          borderColor: '#1a237e',
          backgroundColor: 'rgba(26,35,126,0.1)',
          tension: 0.3,
          fill: true
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } }
      }
    });
  }
}

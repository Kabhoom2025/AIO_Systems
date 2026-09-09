import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { DragDropModule, CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken, getAuthHeaders } from '../../core/auth.helper';

interface SalesSummary { totalSales: number; transactionCount: number; averageTicket: number; }
interface InventoryValuationReport { items: any[]; grandTotal: number; }
interface TopMedicine { medicineId: number; medicineName: string; quantitySold: number; revenue: number; }
interface ReorderSuggestion { medicineId: number; medicineName: string; currentStock: number; suggestedReorderQty: number; }
interface ExpiryRisk { medicineId: number; medicineName: string; batchNumber: string; daysToExpiry: number; riskLevel: string; }

interface WidgetDef { id: string; title: string; route: string; }

const WIDGET_DEFS: WidgetDef[] = [
  { id: 'sales-today',         title: "Today's Sales",       route: 'sales-history' },
  { id: 'inventory-valuation', title: 'Inventory Valuation', route: 'reports' },
  { id: 'notifications',       title: 'Unread Notifications', route: 'reports' },
  { id: 'top-medicines',       title: 'Top-Selling Medicines', route: 'reports' },
  { id: 'reorder-suggestions', title: 'Reorder Suggestions', route: 'ai-insights' },
  { id: 'expiry-risk',         title: 'Expiry Risk',         route: 'ai-insights' }
];

const STORAGE_KEY = 'pharmacy_dashboard_layout';

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
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, DragDropModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  orgId = getOrgIdFromToken();
  widgetDefs = WIDGET_DEFS;

  editMode = signal(false);
  widgetOrder = signal<string[]>(WIDGET_DEFS.map(w => w.id));
  hiddenWidgets = signal<Set<string>>(new Set());

  salesToday = signal<SalesSummary | null>(null);
  inventoryValuation = signal<InventoryValuationReport | null>(null);
  unreadCount = signal(0);
  topMedicines = signal<TopMedicine[]>([]);
  reorderSuggestions = signal<ReorderSuggestion[]>([]);
  expiryRisks = signal<ExpiryRisk[]>([]);

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadLayout();
    this.loadData();
  }

  get visibleWidgetIds(): string[] {
    return this.widgetOrder().filter(id => !this.hiddenWidgets().has(id));
  }

  get hiddenWidgetDefs(): WidgetDef[] {
    return this.widgetDefs.filter(w => this.hiddenWidgets().has(w.id));
  }

  widgetTitle(id: string): string {
    return this.widgetDefs.find(w => w.id === id)?.title ?? id;
  }

  private loadLayout() {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return;
    try {
      const parsed = JSON.parse(raw);
      const knownIds = new Set(this.widgetDefs.map(w => w.id));
      const order: string[] = Array.isArray(parsed.order) ? parsed.order.filter((id: string) => knownIds.has(id)) : [];
      for (const id of knownIds) {
        if (!order.includes(id)) order.push(id);
      }
      this.widgetOrder.set(order);
      const hidden: string[] = Array.isArray(parsed.hidden) ? parsed.hidden.filter((id: string) => knownIds.has(id)) : [];
      this.hiddenWidgets.set(new Set(hidden));
    } catch {
      // ignore malformed stored layout, fall back to defaults already set
    }
  }

  private saveLayout() {
    localStorage.setItem(STORAGE_KEY, JSON.stringify({
      order: this.widgetOrder(),
      hidden: Array.from(this.hiddenWidgets())
    }));
  }

  toggleEditMode() {
    this.editMode.set(!this.editMode());
  }

  drop(event: CdkDragDrop<string[]>) {
    const order = [...this.widgetOrder()];
    const visible = this.visibleWidgetIds;
    const moved = visible[event.previousIndex];
    const target = visible[event.currentIndex];
    const fromIdx = order.indexOf(moved);
    const toIdx = order.indexOf(target);
    moveItemInArray(order, fromIdx, toIdx);
    this.widgetOrder.set(order);
    this.saveLayout();
  }

  hideWidget(id: string) {
    const hidden = new Set(this.hiddenWidgets());
    hidden.add(id);
    this.hiddenWidgets.set(hidden);
    this.saveLayout();
  }

  restoreWidget(id: string) {
    const hidden = new Set(this.hiddenWidgets());
    hidden.delete(id);
    this.hiddenWidgets.set(hidden);
    this.saveLayout();
  }

  resetLayout() {
    localStorage.removeItem(STORAGE_KEY);
    this.widgetOrder.set(this.widgetDefs.map(w => w.id));
    this.hiddenWidgets.set(new Set());
  }

  private loadData() {
    const headers = getAuthHeaders();
    const t = today();

    this.http.get<SalesSummary>(`${environment.apiUrl}/reports/sales-summary/org/${this.orgId}?from=${t}&to=${t}`, { headers })
      .subscribe({ next: d => this.salesToday.set(d), error: () => {} });

    this.http.get<InventoryValuationReport>(`${environment.apiUrl}/reports/inventory-valuation/org/${this.orgId}`, { headers })
      .subscribe({ next: d => this.inventoryValuation.set(d), error: () => {} });

    this.http.get<{ count: number }>(`${environment.apiUrl}/notifications/unread-count/org/${this.orgId}`, { headers })
      .subscribe({ next: d => this.unreadCount.set(d.count), error: () => {} });

    this.http.get<TopMedicine[]>(`${environment.apiUrl}/reports/top-medicines/org/${this.orgId}?from=${firstOfMonth()}&to=${t}&take=5`, { headers })
      .subscribe({ next: d => this.topMedicines.set(d), error: () => {} });

    this.http.get<ReorderSuggestion[]>(`${environment.apiUrl}/ai-insights/reorder-suggestions/org/${this.orgId}`, { headers })
      .subscribe({ next: d => this.reorderSuggestions.set(d.slice(0, 5)), error: () => {} });

    this.http.get<ExpiryRisk[]>(`${environment.apiUrl}/ai-insights/expiry-risk/org/${this.orgId}`, { headers })
      .subscribe({ next: d => this.expiryRisks.set(d.slice(0, 5)), error: () => {} });
  }

  riskColor(level: string) {
    return ({ Expired: '#757575', High: '#e53935', Medium: '#fb8c00', Low: '#43a047' } as any)[level] ?? '#757575';
  }
}

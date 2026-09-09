import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { getOrgIdFromToken, getAuthHeaders } from '../../core/auth.helper';

interface ReorderSuggestion {
  medicineId: number; medicineName: string; currentStock: number;
  avgDailySales: number; daysOfStockLeft: number | null; suggestedReorderQty: number;
}
interface ExpiryRisk {
  medicineId: number; medicineName: string; batchId: number; batchNumber: string;
  quantity: number; daysToExpiry: number; avgDailySales: number;
  projectedDaysToSellOut: number | null; riskLevel: string;
}

@Component({
  selector: 'app-ai-insights',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ai-insights.component.html',
  styleUrl: './ai-insights.component.scss'
})
export class AiInsightsComponent implements OnInit {
  orgId = getOrgIdFromToken();

  reorderSuggestions = signal<ReorderSuggestion[]>([]);
  expiryRisks = signal<ExpiryRisk[]>([]);
  loading = signal(false);

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loading.set(true);
    this.http.get<ReorderSuggestion[]>(`${environment.apiUrl}/ai-insights/reorder-suggestions/org/${this.orgId}`, {
      headers: getAuthHeaders()
    }).subscribe({
      next: data => this.reorderSuggestions.set(data),
      error: () => {}
    });

    this.http.get<ExpiryRisk[]>(`${environment.apiUrl}/ai-insights/expiry-risk/org/${this.orgId}`, {
      headers: getAuthHeaders()
    }).subscribe({
      next: data => { this.expiryRisks.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  riskColor(level: string) {
    return ({ Expired: '#757575', High: '#e53935', Medium: '#fb8c00', Low: '#43a047' } as any)[level] ?? '#757575';
  }
}

import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatIconModule } from '@angular/material/icon';
import { forkJoin, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

interface SummaryStats { todayRevenue:number;todayOrders:number;weekRevenue:number;weekOrders:number;monthRevenue:number;monthOrders:number; }
interface DailySales { date:string;totalRevenue:number;orderCount:number;avgOrderValue:number;completedOrders:number;cancelledOrders:number;hourlyBreakdown:HourlyBreakdown[]; }
interface HourlyBreakdown { hour:number;label:string;orderCount:number;revenue:number; }
interface TopItem { rank:number;itemName:string;categoryName:string;quantitySold:number;revenue:number; }
interface PeakHour { hour:number;label:string;orderCount:number;revenue:number; }

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss',
})
export class ReportsComponent implements OnInit {
  private http = inject(HttpClient);

  loading     = signal(true);
  activeTab   = signal<'overview'|'daily'|'items'|'peak'>('overview');

  summary     = signal<SummaryStats | null>(null);
  daily       = signal<DailySales | null>(null);
  topItems    = signal<TopItem[]>([]);
  peakHours   = signal<PeakHour[]>([]);

  selectedDate = signal(new Date().toISOString().slice(0, 10));

  maxOrderCount = computed(() => Math.max(...(this.peakHours().map(h => h.orderCount)), 1));
  maxHourlyRevenue = computed(() => Math.max(...(this.daily()?.hourlyBreakdown?.map(h => h.revenue) ?? []), 1));
  maxTopQty = computed(() => Math.max(...(this.topItems().map(i => i.quantitySold)), 1));

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll(): void {
    this.loading.set(true);
    const date = this.selectedDate();
    forkJoin({
      summary:  this.http.get<any>(`${environment.apiUrl}/report/summary`).pipe(catchError(() => of({ data: null }))),
      daily:    this.http.get<any>(`${environment.apiUrl}/report/daily-sales?date=${date}`).pipe(catchError(() => of({ data: null }))),
      topItems: this.http.get<any>(`${environment.apiUrl}/report/top-items`).pipe(catchError(() => of({ data: [] }))),
      peak:     this.http.get<any>(`${environment.apiUrl}/report/peak-hours?date=${date}`).pipe(catchError(() => of({ data: [] }))),
    }).subscribe({
      next: ({ summary, daily, topItems, peak }) => {
        this.summary.set(summary.data);
        this.daily.set(daily.data);
        this.topItems.set(topItems.data ?? []);
        this.peakHours.set(peak.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onDateChange(): void {
    const date = this.selectedDate();
    forkJoin({
      daily: this.http.get<any>(`${environment.apiUrl}/report/daily-sales?date=${date}`).pipe(catchError(() => of({ data: null }))),
      peak:  this.http.get<any>(`${environment.apiUrl}/report/peak-hours?date=${date}`).pipe(catchError(() => of({ data: [] }))),
    }).subscribe(({ daily, peak }) => {
      this.daily.set(daily.data);
      this.peakHours.set(peak.data ?? []);
    });
  }

  barWidth(val: number, max: number): number {
    return max > 0 ? Math.round((val / max) * 100) : 0;
  }

  allHours(): HourlyBreakdown[] {
    const d = this.daily();
    if (!d) return [];
    const map = new Map(d.hourlyBreakdown.map(h => [h.hour, h]));
    return Array.from({ length: 24 }, (_, i) => map.get(i) ?? { hour: i, label: this.formatHour(i), orderCount: 0, revenue: 0 });
  }

  formatHour(h: number): string {
    if (h === 0)  return '12 AM';
    if (h < 12)   return `${h} AM`;
    if (h === 12) return '12 PM';
    return `${h - 12} PM`;
  }
}

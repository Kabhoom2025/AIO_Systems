import { Component, OnInit, OnDestroy, inject, signal, computed } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { SystemHealthService } from '../../../core/services/system-health.service';
import { SystemHealthDto, HealthEventDto } from '../../../core/models/system-health.model';

@Component({
  selector: 'app-sa-system-health',
  standalone: true,
  imports: [CommonModule, DatePipe, MatIconModule, MatButtonModule, MatProgressSpinnerModule, MatTooltipModule],
  templateUrl: './sa-system-health.component.html',
  styleUrl: './sa-system-health.component.scss',
})
export class SaSystemHealthComponent implements OnInit, OnDestroy {
  private svc = inject(SystemHealthService);

  health      = signal<SystemHealthDto | null>(null);
  loading     = signal(true);
  error       = signal<string | null>(null);
  lastRefresh = signal<Date | null>(null);
  refreshing  = signal(false);

  private refreshInterval: ReturnType<typeof setInterval> | null = null;

  dbColor = computed(() => this.health()?.dbStatus === 'Connected' ? 'green' : 'red');
  memColor = computed(() => {
    const mb = this.health()?.memoryUsedMb ?? 0;
    return mb > 800 ? 'red' : mb > 400 ? 'yellow' : 'green';
  });
  dbLatencyColor = computed(() => {
    const ms = this.health()?.dbLatencyMs ?? 0;
    return ms > 200 ? 'red' : ms > 80 ? 'yellow' : 'green';
  });

  ngOnInit(): void {
    this.load();
    this.refreshInterval = setInterval(() => this.load(true), 30_000);
  }

  ngOnDestroy(): void {
    if (this.refreshInterval) clearInterval(this.refreshInterval);
  }

  load(silent = false): void {
    if (!silent) this.loading.set(true);
    else         this.refreshing.set(true);
    this.error.set(null);

    this.svc.get().subscribe({
      next: data => {
        this.health.set(data);
        this.lastRefresh.set(new Date());
        this.loading.set(false);
        this.refreshing.set(false);
      },
      error: err => {
        this.error.set(err?.message ?? 'Failed to reach the health endpoint.');
        this.loading.set(false);
        this.refreshing.set(false);
      },
    });
  }

  levelClass(level: string): string { return level.toLowerCase(); }

  levelIcon(level: string): string {
    if (level === 'ERROR') return 'error';
    if (level === 'WARN')  return 'warning';
    return 'info';
  }

  memBar(mb: number): number {
    return Math.min(100, Math.round((mb / 1024) * 100));
  }

  trackEvent(_: number, e: HealthEventDto): string {
    return e.timestamp + e.message;
  }
}

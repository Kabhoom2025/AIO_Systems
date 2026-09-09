import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, of, switchMap, takeWhile, timer } from 'rxjs';

import { ScanApiService } from '../../core/services/scan-api.service';
import { ScanSignalrService } from '../../core/services/scan-signalr.service';
import { ScanDetail, TERMINAL_STATUSES } from '../../core/models/scan.models';

type TabKey =
  | 'overview' | 'url' | 'domain' | 'dns' | 'ssl' | 'redirects' | 'threatIntel' | 'brand' | 'ml' | 'riskFactors';

@Component({
  selector: 'app-scan-detail',
  standalone: true,
  imports: [RouterLink, DatePipe, DecimalPipe],
  providers: [ScanSignalrService],
  templateUrl: './scan-detail.component.html',
  styleUrl: './scan-detail.component.scss'
})
export class ScanDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly scanApi = inject(ScanApiService);
  private readonly signalr = inject(ScanSignalrService);
  private readonly destroyRef = inject(DestroyRef);

  readonly scan = signal<ScanDetail | null>(null);
  readonly loadError = signal<string | null>(null);
  readonly activeTab = signal<TabKey>('overview');
  readonly liveMessage = signal<string | null>(null);

  readonly tabs: { key: TabKey; label: string }[] = [
    { key: 'overview', label: 'Overview' },
    { key: 'url', label: 'URL' },
    { key: 'domain', label: 'Domain' },
    { key: 'dns', label: 'DNS' },
    { key: 'ssl', label: 'SSL/TLS' },
    { key: 'redirects', label: 'Redirects' },
    { key: 'threatIntel', label: 'Threat Intelligence' },
    { key: 'brand', label: 'Brand Detection' },
    { key: 'ml', label: 'ML Analysis' },
    { key: 'riskFactors', label: 'Risk Factors' }
  ];

  ngOnInit(): void {
    const scanId = this.route.snapshot.paramMap.get('id');
    if (!scanId) {
      this.loadError.set('No scan id in the route.');
      return;
    }

    this.signalr.onStageChanged.subscribe((event) => {
      if (event.scanId === scanId) this.liveMessage.set(`${event.stage}: ${event.message}`);
    });
    void this.signalr.connectAndJoin(scanId);
    this.destroyRef.onDestroy(() => void this.signalr.disconnect());

    timer(0, 2000)
      .pipe(
        switchMap(() =>
          this.scanApi.getScan(scanId).pipe(
            catchError(() => {
              this.loadError.set('Could not load this scan.');
              return of(null);
            })
          )
        ),
        takeWhile((scan) => {
          if (scan) this.scan.set(scan);
          return !scan || !TERMINAL_STATUSES.includes(scan.status);
        }, true)
      )
      .subscribe();
  }

  setTab(tab: TabKey): void {
    this.activeTab.set(tab);
  }

  verdictClass(verdict: string | undefined): string {
    return `badge badge--${(verdict ?? 'unknown').toLowerCase()}`;
  }
}

import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subscription, interval, startWith, switchMap } from 'rxjs';
import { RegisteredServiceService } from '../../core/services/registered-service.service';
import { RegisteredService, RunMode } from '../../core/models/registered-service.model';

type ActionState = 'idle' | 'starting' | 'stopping';

@Component({
  selector: 'app-apps',
  imports: [
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
  ],
  templateUrl: './apps.component.html',
  styleUrl: './apps.component.scss',
})
export class AppsComponent implements OnInit, OnDestroy {
  loading = signal(true);
  apps = signal<RegisteredService[]>([]);
  actionStates = signal<Record<number, ActionState>>({});
  displayedColumns = ['app', 'backend', 'frontend', 'status', 'runVia', 'actions'];
  expandedAppId = signal<number | null>(null);

  /** Which mode each app's Run/Stop button uses — defaults to Native, or Docker if that's
   * the only option configured (no native launch fields, only a Docker service name). */
  runModes = signal<Record<number, RunMode>>({});

  /** App id waiting for its backend/frontend to come up so it can be auto-opened. */
  pendingOpenId = signal<number | null>(null);

  private pollSub?: Subscription;

  constructor(private service: RegisteredServiceService) {}

  runMode(appId: number): RunMode {
    return this.runModes()[appId] ?? 'Native';
  }

  setRunMode(appId: number, mode: RunMode): void {
    this.runModes.update((m) => ({ ...m, [appId]: mode }));
  }

  canUseNative(app: RegisteredService): boolean {
    return !!app.backendWorkingDirectory && !!app.backendCommand;
  }

  canUseDocker(app: RegisteredService): boolean {
    return !!app.dockerServiceName;
  }

  /** Defaults each app to Native (or Docker, if that's the only option), and corrects a
   * previously-selected mode that's no longer valid — e.g. its launch fields were cleared. */
  private ensureValidRunModes(apps: RegisteredService[]): void {
    this.runModes.update((modes) => {
      const next = { ...modes };
      for (const app of apps) {
        const current = next[app.id];
        const currentValid = current === 'Docker' ? this.canUseDocker(app) : current === 'Native' ? this.canUseNative(app) : false;
        if (!currentValid) next[app.id] = this.canUseNative(app) ? 'Native' : 'Docker';
      }
      return next;
    });
  }

  ngOnInit(): void {
    // Poll every 4s so status catches up once a dev server finishes compiling
    // (ng serve can take 15-30s) even though start()/stop() return immediately.
    this.pollSub = interval(4000)
      .pipe(
        startWith(0),
        switchMap(() => this.service.getAll())
      )
      .subscribe({
        next: (services) => {
          // Only services registered with SOME way to launch them — native (working
          // directory + command) or Docker (compose service name) — show a card here.
          // Routing/health-only entries with neither don't belong on this page.
          const apps = services.filter((s) => this.canUseNative(s) || this.canUseDocker(s));
          this.apps.set(apps);
          this.loading.set(false);
          this.clearSettledActionStates(apps);
          this.checkPendingOpen(apps);
          this.ensureValidRunModes(apps);
        },
        error: () => this.loading.set(false),
      });
  }

  ngOnDestroy(): void {
    this.pollSub?.unsubscribe();
  }

  private clearSettledActionStates(apps: RegisteredService[]): void {
    this.actionStates.update((states) => {
      const next = { ...states };
      for (const app of apps) {
        const state = next[app.id];
        const fullyUp = app.backendRunning && (!app.frontendCommand || app.frontendRunning);
        const fullyDown = !app.backendRunning && !app.frontendRunning;
        if (state === 'starting' && fullyUp) delete next[app.id];
        if (state === 'stopping' && fullyDown) delete next[app.id];
      }
      return next;
    });
  }

  actionState(appId: number): ActionState {
    return this.actionStates()[appId] ?? 'idle';
  }

  isFullyUp(app: RegisteredService): boolean {
    return app.backendRunning && (!app.frontendCommand || app.frontendRunning);
  }

  isFullyDown(app: RegisteredService): boolean {
    return !app.backendRunning && !app.frontendRunning;
  }

  /** Count of apps considered fully up, for the header's "N / total running" summary. */
  runningCount(): number {
    return this.apps().filter((a) => this.isFullyUp(a)).length;
  }

  toggleExpanded(appId: number): void {
    this.expandedAppId.set(this.expandedAppId() === appId ? null : appId);
  }

  isExpanded(appId: number): boolean {
    return this.expandedAppId() === appId;
  }

  run(app: RegisteredService): void {
    this.actionStates.update((s) => ({ ...s, [app.id]: 'starting' }));
    this.service.start(app.id, this.runMode(app.id)).subscribe({
      next: (updated) => this.replaceApp(updated),
      error: (err) => {
        this.actionStates.update((s) => ({ ...s, [app.id]: 'idle' }));
        alert(err.error?.message ?? 'Failed to start this app — see server logs for details.');
      },
    });
  }

  /** Opens the app — starting it first if needed, so the user never lands on a connection-refused page. */
  open(app: RegisteredService): void {
    const url = app.frontendUrl || app.baseUrl;
    if (this.isFullyUp(app)) {
      window.open(url, '_blank');
      return;
    }
    this.pendingOpenId.set(app.id);
    if (this.actionState(app.id) === 'idle') this.run(app);
  }

  private checkPendingOpen(apps: RegisteredService[]): void {
    const pendingId = this.pendingOpenId();
    if (pendingId == null) return;
    const app = apps.find((a) => a.id === pendingId);
    if (app && this.isFullyUp(app)) {
      this.pendingOpenId.set(null);
      window.open(app.frontendUrl || app.baseUrl, '_blank');
    }
  }

  stop(app: RegisteredService): void {
    this.actionStates.update((s) => ({ ...s, [app.id]: 'stopping' }));
    this.service.stop(app.id, this.runMode(app.id)).subscribe({
      next: (updated) => this.replaceApp(updated),
      error: (err) => {
        this.actionStates.update((s) => ({ ...s, [app.id]: 'idle' }));
        alert(err.error?.message ?? 'Failed to stop this app — see server logs for details.');
      },
    });
  }

  private replaceApp(updated: RegisteredService): void {
    this.apps.update((list) => list.map((a) => (a.id === updated.id ? updated : a)));
    this.clearSettledActionStates(this.apps());
  }
}

import { Component, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RegisteredServiceService } from '../../core/services/registered-service.service';
import { RegisteredService } from '../../core/models/registered-service.model';
import {
  RegisteredServiceFormDialogComponent,
  RegisteredServiceFormResult,
} from './registered-service-form-dialog.component';

type PingState = 'idle' | 'checking' | 'online' | 'offline';

@Component({
  selector: 'app-registered-services',
  imports: [
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
  ],
  templateUrl: './registered-services.component.html',
  styleUrl: './registered-services.component.scss',
})
export class RegisteredServicesComponent implements OnInit {
  loading = signal(true);
  services = signal<RegisteredService[]>([]);
  pingStates = signal<Record<number, PingState>>({});
  displayedColumns = ['name', 'routePrefix', 'baseUrl', 'moduleKeys', 'isActive', 'health', 'actions'];

  constructor(private service: RegisteredServiceService, private dialog: MatDialog) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: (services) => {
        this.services.set(services);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  pingState(id: number): PingState {
    return this.pingStates()[id] ?? 'idle';
  }

  checkHealth(service: RegisteredService): void {
    if (!service.healthCheckPath) return;
    this.pingStates.update((s) => ({ ...s, [service.id]: 'checking' }));
    this.service.checkHealth(service).subscribe({
      next: () => this.pingStates.update((s) => ({ ...s, [service.id]: 'online' })),
      error: () => this.pingStates.update((s) => ({ ...s, [service.id]: 'offline' })),
    });
  }

  openCreateDialog(): void {
    const ref = this.dialog.open(RegisteredServiceFormDialogComponent, {
      width: '560px',
      maxHeight: '90vh',
    });
    ref.afterClosed().subscribe((result: RegisteredServiceFormResult | undefined) => {
      if (!result) return;
      this.service.create(result).subscribe(() => this.load());
    });
  }

  openEditDialog(service: RegisteredService): void {
    const ref = this.dialog.open(RegisteredServiceFormDialogComponent, {
      width: '560px',
      maxHeight: '90vh',
      data: { service },
    });
    ref.afterClosed().subscribe((result: RegisteredServiceFormResult | undefined) => {
      if (!result) return;
      this.service.update(service.id, result).subscribe(() => this.load());
    });
  }

  deleteService(service: RegisteredService): void {
    if (!confirm(`Remove registered service "${service.name}"?`)) return;
    this.service.delete(service.id).subscribe(() => this.load());
  }
}

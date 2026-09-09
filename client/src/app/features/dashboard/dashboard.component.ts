import { Component, OnInit, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { OrganizationService } from '../../core/services/organization.service';
import { PlatformModuleService } from '../../core/services/platform-module.service';
import { RegisteredServiceService } from '../../core/services/registered-service.service';
import { Organization } from '../../core/models/organization.model';
import { PlatformModule } from '../../core/models/platform-module.model';
import { RegisteredService } from '../../core/models/registered-service.model';

interface ServiceStatus {
  service: RegisteredService;
  status: 'checking' | 'online' | 'offline' | 'unconfigured';
}

@Component({
  selector: 'app-dashboard',
  imports: [MatCardModule, MatIconModule, MatChipsModule, MatProgressSpinnerModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  loading = signal(true);
  organizations = signal<Organization[]>([]);
  platformModules = signal<PlatformModule[]>([]);
  serviceStatuses = signal<ServiceStatus[]>([]);

  constructor(
    private orgService: OrganizationService,
    private moduleService: PlatformModuleService,
    private registeredServiceService: RegisteredServiceService
  ) {}

  ngOnInit(): void {
    forkJoin({
      orgs: this.orgService.getAll().pipe(catchError(() => of([]))),
      modules: this.moduleService.getAll().pipe(catchError(() => of([]))),
      services: this.registeredServiceService.getAll().pipe(catchError(() => of([]))),
    }).subscribe(({ orgs, modules, services }) => {
      this.organizations.set(orgs);
      this.platformModules.set(modules);
      this.loading.set(false);

      this.serviceStatuses.set(
        services.map((service) => ({ service, status: 'checking' as const }))
      );
      services.forEach((service) => this.pingService(service));
    });
  }

  private pingService(service: RegisteredService): void {
    if (!service.healthCheckPath) {
      this.updateStatus(service.id, 'unconfigured');
      return;
    }
    this.registeredServiceService.checkHealth(service).subscribe({
      next: () => this.updateStatus(service.id, 'online'),
      error: () => this.updateStatus(service.id, 'offline'),
    });
  }

  private updateStatus(serviceId: number, status: ServiceStatus['status']): void {
    this.serviceStatuses.update((list) =>
      list.map((s) => (s.service.id === serviceId ? { ...s, status } : s))
    );
  }

  get activeOrgCount(): number {
    return this.organizations().filter((o) => o.isActive).length;
  }
}

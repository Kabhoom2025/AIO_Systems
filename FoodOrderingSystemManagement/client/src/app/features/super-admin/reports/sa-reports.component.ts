import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { OrganizationService } from '../../../core/services/organization.service';
import { Organization, OrgReport } from '../../../core/models/organization.model';

interface BranchWithReport {
  org: Organization;
  report: OrgReport | null;
  loading: boolean;
  error: boolean;
}

@Component({
  selector: 'app-sa-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatButtonModule,
            MatProgressSpinnerModule, MatTooltipModule, CurrencyPipe],
  templateUrl: './sa-reports.component.html',
  styleUrl: './sa-reports.component.scss',
})
export class SaReportsComponent implements OnInit {
  private orgSvc = inject(OrganizationService);
  private snack  = inject(MatSnackBar);

  branches     = signal<BranchWithReport[]>([]);
  loading      = signal(true);
  searchTerm   = signal('');

  filtered = computed(() => {
    const q = this.searchTerm().toLowerCase();
    return q
      ? this.branches().filter(b => b.org.name.toLowerCase().includes(q))
      : this.branches();
  });

  totalRevenue    = computed(() => this.branches().reduce((s, b) => s + (b.report?.totalRevenue ?? 0), 0));
  totalOrders     = computed(() => this.branches().reduce((s, b) => s + (b.report?.totalOrders ?? 0), 0));
  totalUsers      = computed(() => this.branches().reduce((s, b) => s + (b.report?.activeUsers ?? 0), 0));
  todayRevenue    = computed(() => this.branches().reduce((s, b) => s + (b.report?.todayRevenue ?? 0), 0));

  ngOnInit(): void {
    this.orgSvc.getAll().subscribe({
      next: orgs => {
        const initial = orgs.map(o => ({ org: o, report: null, loading: true, error: false }));
        this.branches.set(initial);
        this.loading.set(false);
        this.loadAllReports(orgs);
      },
      error: () => {
        this.loading.set(false);
        this.snack.open('Failed to load branches', '', { duration: 3000, panelClass: 'snack-error' });
      },
    });
  }

  private loadAllReports(orgs: Organization[]): void {
    orgs.forEach(org => {
      this.orgSvc.getOrgReport(org.id).pipe(
        catchError(() => of(null))
      ).subscribe(report => {
        this.branches.update(list =>
          list.map(b => b.org.id === org.id
            ? { ...b, report, loading: false, error: report === null }
            : b
          )
        );
      });
    });
  }

  refresh(): void {
    this.loading.set(true);
    this.branches.set([]);
    this.ngOnInit();
  }

  orgInitial(name: string): string { return name.charAt(0).toUpperCase(); }
}

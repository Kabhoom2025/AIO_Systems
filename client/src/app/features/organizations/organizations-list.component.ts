import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router } from '@angular/router';
import { OrganizationService } from '../../core/services/organization.service';
import { Organization } from '../../core/models/organization.model';
import {
  OrganizationFormDialogComponent,
  OrganizationFormResult,
} from './organization-form-dialog.component';

@Component({
  selector: 'app-organizations-list',
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
  ],
  templateUrl: './organizations-list.component.html',
  styleUrl: './organizations-list.component.scss',
})
export class OrganizationsListComponent implements OnInit {
  loading = signal(true);
  organizations = signal<Organization[]>([]);
  displayedColumns = ['name', 'tenantKey', 'email', 'userCount', 'isActive', 'createdDate', 'actions'];

  constructor(
    private orgService: OrganizationService,
    private dialog: MatDialog,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.orgService.getAll().subscribe({
      next: (orgs) => {
        this.organizations.set(orgs);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreateDialog(): void {
    const ref = this.dialog.open(OrganizationFormDialogComponent, { width: '520px' });
    ref.afterClosed().subscribe((result: OrganizationFormResult | undefined) => {
      if (!result) return;
      this.orgService.create(result).subscribe(() => this.load());
    });
  }

  openEditDialog(org: Organization): void {
    const ref = this.dialog.open(OrganizationFormDialogComponent, {
      width: '520px',
      data: { organization: org },
    });
    ref.afterClosed().subscribe((result: OrganizationFormResult | undefined) => {
      if (!result) return;
      this.orgService.update(org.id, result).subscribe(() => this.load());
    });
  }

  deleteOrg(org: Organization): void {
    if (!confirm(`Delete organization "${org.name}"? This cannot be undone.`)) return;
    this.orgService.delete(org.id).subscribe(() => this.load());
  }

  viewDetail(org: Organization): void {
    this.router.navigate(['/admin/organizations', org.id]);
  }
}

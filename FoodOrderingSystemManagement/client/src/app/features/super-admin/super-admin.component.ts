import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { OrganizationService } from '../../core/services/organization.service';
import { BranchService } from '../../core/services/branch.service';
import { Organization, OrgUser } from '../../core/models/organization.model';
import { Branch, BranchReport } from '../../core/models/branch.model';
import { OrganizationFormDialogComponent } from './organization-form-dialog.component';
import { OrgAdminDialogComponent } from './org-admin-dialog.component';
import { BranchFormDialogComponent } from './branch-form-dialog.component';
import { ConfirmationDialogComponent } from '../../shared/components/confirmation-dialog/confirmation-dialog.component';

@Component({
  selector: 'app-super-admin',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatButtonModule,
            MatTooltipModule, MatProgressSpinnerModule, CurrencyPipe],
  templateUrl: './super-admin.component.html',
  styleUrl: './super-admin.component.scss',
})
export class SuperAdminComponent implements OnInit {
  private orgSvc    = inject(OrganizationService);
  private branchSvc = inject(BranchService);
  private dialog = inject(MatDialog);
  private snack  = inject(MatSnackBar);

  organizations = signal<Organization[]>([]);
  loading       = signal(false);
  searchTerm    = signal('');
  filterActive  = signal<'all' | 'active' | 'inactive'>('all');
  selectedId    = signal<number | null>(null);
  orgUsers      = signal<OrgUser[]>([]);
  usersLoading  = signal(false);
  branches        = signal<Branch[]>([]);
  branchesLoading = signal(false);
  branchReports   = signal<Map<number, BranchReport>>(new Map());

  filtered = computed(() => {
    let list = this.organizations();
    const q = this.searchTerm().toLowerCase();
    if (q) list = list.filter(o => o.name.toLowerCase().includes(q) || o.email?.toLowerCase().includes(q));
    if (this.filterActive() === 'active')   list = list.filter(o => o.isActive);
    if (this.filterActive() === 'inactive') list = list.filter(o => !o.isActive);
    return list;
  });

  selectedOrg  = computed(() => this.organizations().find(o => o.id === this.selectedId()) ?? null);
  totalOrgs    = computed(() => this.organizations().length);
  activeOrgs   = computed(() => this.organizations().filter(o => o.isActive).length);
  totalUsers   = computed(() => this.organizations().reduce((s, o) => s + o.userCount, 0));

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.orgSvc.getAll().subscribe({
      next: orgs => { this.organizations.set(orgs); this.loading.set(false); },
      error: () => { this.loading.set(false); this.snack.open('Failed to load organizations', '', { duration: 3000, panelClass: 'snack-error' }); },
    });
  }

  selectOrg(id: number): void {
    if (this.selectedId() === id) { this.selectedId.set(null); return; }
    this.selectedId.set(id);
    this.loadOrgUsers(id);
    this.loadBranches(id);
  }

  loadOrgUsers(id: number): void {
    this.usersLoading.set(true);
    this.orgUsers.set([]);
    this.orgSvc.getOrgUsers(id).subscribe({
      next: users => { this.orgUsers.set(users); this.usersLoading.set(false); },
      error: () => this.usersLoading.set(false),
    });
  }

  confirmDeleteUser(user: OrgUser, event: Event): void {
    event.stopPropagation();
    const org = this.selectedOrg();
    if (!org) return;
    this.dialog.open(ConfirmationDialogComponent, {
      data: {
        title: 'Delete User',
        message: `Are you sure you want to delete "${user.name}" (${user.roleName})? This action cannot be undone.`,
        confirmText: 'Delete',
        danger: true,
      },
      width: '420px',
    }).afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.orgSvc.deleteOrgUser(org.id, user.id).subscribe({
        next: () => {
          this.orgUsers.update(list => list.filter(u => u.id !== user.id));
          this.organizations.update(list => list.map(o => o.id === org.id ? { ...o, userCount: o.userCount - 1 } : o));
          this.snack.open(`"${user.name}" deleted`, '', { duration: 2500, panelClass: 'snack-success' });
        },
        error: err => {
          const msg = err?.error?.message || err?.error?.Message || 'Failed to delete user';
          this.snack.open(msg, '', { duration: 4000, panelClass: 'snack-error' });
        },
      });
    });
  }

  loadBranches(id: number): void {
    this.branchesLoading.set(true);
    this.branches.set([]);
    this.branchReports.set(new Map());
    this.branchSvc.getByOrganization(id).subscribe({
      next: res => { this.branches.set(res.data); this.branchesLoading.set(false); },
      error: () => this.branchesLoading.set(false),
    });
    this.branchSvc.getReports(id).subscribe({
      next: res => this.branchReports.set(new Map(res.data.map(r => [r.branchId, r]))),
      error: () => {},
    });
  }

  branchReportFor(branchId: number): BranchReport | undefined {
    return this.branchReports().get(branchId);
  }

  openAddBranch(org: Organization, event: Event): void {
    event.stopPropagation();
    this.dialog.open(BranchFormDialogComponent, { data: { organization: org }, disableClose: true })
      .afterClosed().subscribe(result => {
        if (!result) return;
        this.branchSvc.create({ ...result, organizationId: org.id }).subscribe({
          next: res => {
            this.branches.update(list => [...list, res.data]);
            this.snack.open(`Branch "${res.data.name}" created`, '', { duration: 3000, panelClass: 'snack-success' });
          },
          error: err => {
            const msg = err?.error?.message || err?.error?.Message || 'Failed to create branch';
            this.snack.open(msg, '', { duration: 4000, panelClass: 'snack-error' });
          },
        });
      });
  }

  openEditBranch(branch: Branch, event: Event): void {
    event.stopPropagation();
    const org = this.selectedOrg();
    if (!org) return;
    this.dialog.open(BranchFormDialogComponent, { data: { organization: org, branch }, disableClose: true })
      .afterClosed().subscribe(result => {
        if (!result) return;
        this.branchSvc.update(branch.id, result).subscribe({
          next: res => {
            this.branches.update(list => list.map(b => b.id === res.data.id ? res.data : b));
            this.snack.open('Branch updated', '', { duration: 2500, panelClass: 'snack-success' });
          },
          error: err => {
            const msg = err?.error?.message || err?.error?.Message || 'Failed to update branch';
            this.snack.open(msg, '', { duration: 4000, panelClass: 'snack-error' });
          },
        });
      });
  }

  confirmDeleteBranch(branch: Branch, event: Event): void {
    event.stopPropagation();
    if (branch.isDefault) {
      this.snack.open('The default branch cannot be deleted.', '', { duration: 3000, panelClass: 'snack-error' });
      return;
    }
    this.dialog.open(ConfirmationDialogComponent, {
      data: {
        title: 'Delete Branch',
        message: `Are you sure you want to delete "${branch.name}"? This action cannot be undone.`,
        confirmText: 'Delete',
        danger: true,
      },
      width: '420px',
    }).afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.branchSvc.delete(branch.id).subscribe({
        next: () => {
          this.branches.update(list => list.filter(b => b.id !== branch.id));
          this.snack.open('Branch deleted', '', { duration: 2500, panelClass: 'snack-success' });
        },
        error: err => {
          const msg = err?.error?.message || err?.error?.Message || 'Failed to delete branch';
          this.snack.open(msg, '', { duration: 4000, panelClass: 'snack-error' });
        },
      });
    });
  }

  openCreate(): void {
    this.dialog.open(OrganizationFormDialogComponent, { data: {}, disableClose: true })
      .afterClosed().subscribe(result => {
        if (!result) return;
        this.orgSvc.create(result).subscribe({
          next: org => { this.organizations.update(list => [...list, org]); this.snack.open('Organization created', '', { duration: 2500, panelClass: 'snack-success' }); },
          error: () => this.snack.open('Failed to create organization', '', { duration: 3000, panelClass: 'snack-error' }),
        });
      });
  }

  openEdit(org: Organization, event: Event): void {
    event.stopPropagation();
    this.dialog.open(OrganizationFormDialogComponent, { data: { organization: org }, disableClose: true })
      .afterClosed().subscribe(result => {
        if (!result) return;
        this.orgSvc.update(org.id, result).subscribe({
          next: updated => {
            this.organizations.update(list => list.map(o => o.id === updated.id ? updated : o));
            this.snack.open('Organization updated', '', { duration: 2500, panelClass: 'snack-success' });
          },
          error: () => this.snack.open('Failed to update organization', '', { duration: 3000, panelClass: 'snack-error' }),
        });
      });
  }

  confirmDelete(org: Organization, event: Event): void {
    event.stopPropagation();
    this.dialog.open(ConfirmationDialogComponent, {
      data: {
        title: 'Delete Organization',
        message: `Are you sure you want to delete "${org.name}"? All users in this organization will be affected. This action cannot be undone.`,
        confirmText: 'Delete',
        danger: true,
      },
      width: '420px',
    }).afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.orgSvc.delete(org.id).subscribe({
        next: () => {
          this.organizations.update(list => list.filter(o => o.id !== org.id));
          if (this.selectedId() === org.id) this.selectedId.set(null);
          this.snack.open('Organization deleted', '', { duration: 2500, panelClass: 'snack-success' });
        },
        error: err => {
          const msg = err?.error?.message || err?.error?.Message || 'Failed to delete organization';
          this.snack.open(msg, '', { duration: 4000, panelClass: 'snack-error' });
        },
      });
    });
  }

  openAddAdmin(org: Organization, event: Event): void {
    event.stopPropagation();
    this.branchSvc.getByOrganization(org.id).subscribe({
      next: res => this.launchAddAdminDialog(org, res.data),
      error: () => this.launchAddAdminDialog(org, []),
    });
  }

  private launchAddAdminDialog(org: Organization, branches: Branch[]): void {
    this.dialog.open(OrgAdminDialogComponent, { data: { organization: org, branches }, disableClose: true })
      .afterClosed().subscribe(result => {
        if (!result) return;
        this.orgSvc.createOrgAdmin(org.id, result).subscribe({
          next: user => {
            this.orgUsers.update(list => [...list, user]);
            this.organizations.update(list => list.map(o => o.id === org.id ? { ...o, userCount: o.userCount + 1 } : o));
            this.snack.open(`Admin "${user.name}" created`, '', { duration: 3000, panelClass: 'snack-success' });
          },
          error: err => {
            const msg = err?.error?.message || err?.error?.Message || 'Failed to create admin';
            this.snack.open(msg, '', { duration: 4000, panelClass: 'snack-error' });
          },
        });
      });
  }

  orgInitial(name: string): string { return name.charAt(0).toUpperCase(); }

  roleColor(role: string): string {
    const map: Record<string, string> = { Admin: '#1a237e', Cashier: '#006064', Waiter: '#4a148c', InventoryManager: '#1b5e20' };
    return map[role] ?? '#546e7a';
  }

  roleBg(role: string): string {
    const map: Record<string, string> = { Admin: '#e8eaf6', Cashier: '#e0f7fa', Waiter: '#f3e5f5', InventoryManager: '#e8f5e9' };
    return map[role] ?? '#eceff1';
  }
}

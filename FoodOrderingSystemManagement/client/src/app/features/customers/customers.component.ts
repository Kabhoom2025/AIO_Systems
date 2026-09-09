import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';

import { CustomerService } from '../../core/services/customer.service';
import { AuthService } from '../../core/authentication/auth.service';
import { APP_CONSTANTS } from '../../core/constants/app.constants';
import { HasPermissionDirective } from '../../core/directives/has-permission.directive';
import { NotificationService } from '../../shared/services/notification.service';
import { ConfirmationDialogComponent } from '../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { Customer } from '../../core/models/customer.model';
import { CustomerFormDialogComponent } from './customer-form-dialog.component';
import { CustomerDetailDialogComponent } from './customer-detail-dialog.component';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatCardModule, MatTableModule, MatButtonModule, MatIconModule,
    MatInputModule, MatFormFieldModule, MatProgressSpinnerModule,
    MatTooltipModule, MatDialogModule, MatChipsModule,
    HasPermissionDirective,
  ],
  templateUrl: './customers.component.html',
  styleUrls: ['./customers.component.scss'],
})
export class CustomersComponent implements OnInit {
  private readonly service = inject(CustomerService);
  private readonly dialog  = inject(MatDialog);
  private readonly notify  = inject(NotificationService);
  private readonly auth    = inject(AuthService);

  get isAdmin(): boolean { return this.auth.userRole() === APP_CONSTANTS.ROLES.ADMIN; }

  loading  = signal(false);
  deleting = signal<number | null>(null);
  customers = signal<Customer[]>([]);
  searchTerm = signal('');

  displayedColumns = ['name', 'phone', 'tier', 'points', 'spend', 'visits', 'status', 'actions'];

  filtered = computed(() => {
    const q = this.searchTerm().toLowerCase();
    if (!q) return this.customers();
    return this.customers().filter(
      c => c.name.toLowerCase().includes(q) || c.phone.includes(q) || (c.email ?? '').toLowerCase().includes(q)
    );
  });

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: res => {
        this.customers.set(res.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onSearch(val: string) {
    this.searchTerm.set(val);
  }

  openCreate() {
    const ref = this.dialog.open(CustomerFormDialogComponent, { width: '480px', data: null });
    ref.afterClosed().subscribe(created => { if (created) this.load(); });
  }

  openEdit(customer: Customer) {
    const ref = this.dialog.open(CustomerFormDialogComponent, { width: '480px', data: customer });
    ref.afterClosed().subscribe(updated => { if (updated) this.load(); });
  }

  openDetail(customer: Customer) {
    const ref = this.dialog.open(CustomerDetailDialogComponent, { width: '680px', data: customer });
    ref.afterClosed().subscribe(() => this.load());
  }

  deleteCustomer(customer: Customer) {
    const ref = this.dialog.open(ConfirmationDialogComponent, {
      data: {
        title: 'Delete Customer',
        message: `Permanently delete "${customer.name}"? All associated data will be removed. This cannot be undone.`,
        confirmText: 'Delete',
        danger: true,
      },
      width: '420px',
    });
    ref.afterClosed().subscribe(confirmed => {
      if (!confirmed) return;
      this.deleting.set(customer.id);
      this.service.delete(customer.id).subscribe({
        next: () => {
          this.notify.success(`Customer "${customer.name}" deleted.`);
          this.deleting.set(null);
          this.load();
        },
        error: err => {
          this.notify.error(err?.error?.message || 'Failed to delete customer.');
          this.deleting.set(null);
        },
      });
    });
  }

  goldCount   = computed(() => this.customers().filter(c => c.tier === 'Gold').length);
  totalPoints = computed(() => this.customers().reduce((s, c) => s + c.loyaltyPoints, 0));

  tierColor(tier: string): string {
    return tier === 'Gold' ? 'gold' : tier === 'Silver' ? 'silver' : 'bronze';
  }

  tierIcon(tier: string): string {
    return tier === 'Gold' ? 'emoji_events' : tier === 'Silver' ? 'workspace_premium' : 'military_tech';
  }
}

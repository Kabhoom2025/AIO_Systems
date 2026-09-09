import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TabViewModule } from 'primeng/tabview';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { NotificationService } from '../../../core/notification.service';
import { SalesOrderDto } from '../../../core/sales-order-api.service';
import { CustomerInvoiceDto } from '../../../core/customer-invoice-api.service';
import { CustomerPortalApiService, MyContactDto } from '../../../core/customer-portal-api.service';

@Component({
  selector: 'app-my-customer-portal',
  standalone: true,
  imports: [CommonModule, TabViewModule, TableModule, TagModule, ToastModule],
  templateUrl: './my-customer-portal.component.html',
  styleUrl: './my-customer-portal.component.scss'
})
export class MyCustomerPortalComponent implements OnInit {
  loading = false;
  noContactLinked = false;

  profile: MyContactDto | null = null;
  orders: SalesOrderDto[] = [];
  invoices: CustomerInvoiceDto[] = [];

  constructor(
    private api: CustomerPortalApiService,
    private notify: NotificationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.noContactLinked = false;
    this.api.getMyProfile().subscribe({
      next: profile => {
        this.profile = profile;
        this.loading = false;
        this.loadOrders();
        this.loadInvoices();
      },
      error: err => {
        this.loading = false;
        if (err.status === 404) {
          this.noContactLinked = true;
        } else {
          this.notify.error(err.error?.message ?? 'Failed to load your profile.');
        }
      }
    });
  }

  private loadOrders() {
    this.api.getMyOrders().subscribe({
      next: rows => (this.orders = rows),
      error: err => this.notify.error(err.error?.message ?? 'Failed to load your orders.')
    });
  }

  private loadInvoices() {
    this.api.getMyInvoices().subscribe({
      next: rows => (this.invoices = rows),
      error: err => this.notify.error(err.error?.message ?? 'Failed to load your invoices.')
    });
  }

  orderStatusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Confirmed') return 'success';
    if (status === 'Cancelled') return 'danger';
    return 'info';
  }

  invoiceStatusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Paid') return 'success';
    if (status === 'Voided') return 'danger';
    if (status === 'Sent') return 'warn';
    return 'info';
  }
}

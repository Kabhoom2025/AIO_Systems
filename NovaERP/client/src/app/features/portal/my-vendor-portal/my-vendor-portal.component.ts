import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TabViewModule } from 'primeng/tabview';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { NotificationService } from '../../../core/notification.service';
import { PurchaseOrderDto } from '../../../core/purchase-order-api.service';
import { VendorBillDto } from '../../../core/vendor-bill-api.service';
import { VendorPortalApiService, MyVendorDto } from '../../../core/vendor-portal-api.service';

@Component({
  selector: 'app-my-vendor-portal',
  standalone: true,
  imports: [CommonModule, TabViewModule, TableModule, TagModule, ToastModule],
  templateUrl: './my-vendor-portal.component.html',
  styleUrl: './my-vendor-portal.component.scss'
})
export class MyVendorPortalComponent implements OnInit {
  loading = false;
  noVendorLinked = false;

  profile: MyVendorDto | null = null;
  purchaseOrders: PurchaseOrderDto[] = [];
  bills: VendorBillDto[] = [];

  constructor(
    private api: VendorPortalApiService,
    private notify: NotificationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.noVendorLinked = false;
    this.api.getMyProfile().subscribe({
      next: profile => {
        this.profile = profile;
        this.loading = false;
        this.loadPurchaseOrders();
        this.loadBills();
      },
      error: err => {
        this.loading = false;
        if (err.status === 404) {
          this.noVendorLinked = true;
        } else {
          this.notify.error(err.error?.message ?? 'Failed to load your profile.');
        }
      }
    });
  }

  private loadPurchaseOrders() {
    this.api.getMyPurchaseOrders().subscribe({
      next: rows => (this.purchaseOrders = rows),
      error: err => this.notify.error(err.error?.message ?? 'Failed to load your purchase orders.')
    });
  }

  private loadBills() {
    this.api.getMyBills().subscribe({
      next: rows => (this.bills = rows),
      error: err => this.notify.error(err.error?.message ?? 'Failed to load your bills.')
    });
  }

  orderStatusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Received') return 'success';
    if (status === 'Cancelled') return 'danger';
    if (status === 'Confirmed') return 'warn';
    return 'info';
  }

  billStatusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Paid') return 'success';
    if (status === 'Voided') return 'danger';
    if (status === 'Approved') return 'warn';
    return 'info';
  }
}

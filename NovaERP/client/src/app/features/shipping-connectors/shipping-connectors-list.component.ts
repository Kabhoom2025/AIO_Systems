import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import { ShippingConnectorApiService, ShippingConnectorDto } from '../../core/shipping-connector-api.service';

@Component({
  selector: 'app-shipping-connectors-list',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule, TagModule, ToastModule, ConfirmDialogModule, HasPermissionDirective],
  providers: [ConfirmationService],
  templateUrl: './shipping-connectors-list.component.html',
  styleUrl: './shipping-connectors-list.component.scss'
})
export class ShippingConnectorsListComponent implements OnInit {
  connectors: ShippingConnectorDto[] = [];
  loading = false;

  constructor(
    private api: ShippingConnectorApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.connectors = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load shipping connectors.');
      }
    });
  }

  openNew() {
    this.router.navigate(['/shipping-connectors', 'new']);
  }

  openEdit(c: ShippingConnectorDto) {
    this.router.navigate(['/shipping-connectors', c.id]);
  }

  delete(c: ShippingConnectorDto) {
    this.confirm.confirm({
      message: `Delete connector "${c.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(c.id).subscribe({
          next: () => {
            this.notify.success('Connector deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete connector.')
        });
      }
    });
  }
}

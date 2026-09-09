import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { ToastModule } from 'primeng/toast';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  StockTransferApiService, StockTransferDto, CreateStockTransferDto
} from '../../../core/stock-transfer-api.service';
import { ProductApiService, ProductDto } from '../../../core/product-api.service';
import { WarehouseApiService, WarehouseDto } from '../../../core/warehouse-api.service';

@Component({
  selector: 'app-stock-transfers',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, DropdownModule, ToastModule, HasPermissionDirective
  ],
  templateUrl: './stock-transfers.component.html',
  styleUrl: './stock-transfers.component.scss'
})
export class StockTransfersComponent implements OnInit {
  transfers: StockTransferDto[] = [];
  products: ProductDto[] = [];
  warehouses: WarehouseDto[] = [];
  loading = false;

  showDialog = false;
  saving = false;
  form: { productId: number | null; fromWarehouseId: number | null; toWarehouseId: number | null; quantity: number | null; notes: string | null } = this.emptyForm();

  constructor(
    private api: StockTransferApiService,
    private productApi: ProductApiService,
    private warehouseApi: WarehouseApiService,
    private notify: NotificationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.productApi.getAll().subscribe({ next: rows => (this.products = rows), error: () => (this.products = []) });
    this.warehouseApi.getAll().subscribe({ next: rows => (this.warehouses = rows), error: () => (this.warehouses = []) });
  }

  private emptyForm() {
    return { productId: null, fromWarehouseId: null, toWarehouseId: null, quantity: null, notes: null };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.transfers = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load stock transfers.');
      }
    });
  }

  openNew() {
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  save() {
    if (!this.form.productId || !this.form.fromWarehouseId || !this.form.toWarehouseId || !this.form.quantity) {
      this.notify.warn('Product, both warehouses, and a positive quantity are required.');
      return;
    }
    if (this.form.fromWarehouseId === this.form.toWarehouseId) {
      this.notify.warn('From and To warehouses must be different.');
      return;
    }
    this.saving = true;
    this.api.create({
      productId: this.form.productId,
      fromWarehouseId: this.form.fromWarehouseId,
      toWarehouseId: this.form.toWarehouseId,
      quantity: this.form.quantity,
      notes: this.form.notes
    } as CreateStockTransferDto).subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success('Stock transfer recorded.');
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to record stock transfer.');
      }
    });
  }
}

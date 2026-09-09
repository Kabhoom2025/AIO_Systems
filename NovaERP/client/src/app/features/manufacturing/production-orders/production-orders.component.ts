import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  ProductionOrderApiService, ProductionOrderDto, CreateProductionOrderDto, UpdateProductionOrderDto
} from '../../../core/production-order-api.service';
import { ProductApiService, ProductDto } from '../../../core/product-api.service';
import { WarehouseApiService, WarehouseDto } from '../../../core/warehouse-api.service';
import { UserApiService, UserDto } from '../../../core/user-api.service';

@Component({
  selector: 'app-production-orders',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputNumberModule, DropdownModule, CalendarModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './production-orders.component.html',
  styleUrl: './production-orders.component.scss'
})
export class ProductionOrdersComponent implements OnInit {
  orders: ProductionOrderDto[] = [];
  products: ProductDto[] = [];
  warehouses: WarehouseDto[] = [];
  users: UserDto[] = [];
  loading = false;

  showDialog = false;
  editing: ProductionOrderDto | null = null;
  saving = false;

  form: { productId: number | null; warehouseId: number | null; quantity: number | null; orderDate: Date | null; ownerId: number | null } = this.emptyForm();

  constructor(
    private api: ProductionOrderApiService,
    private productApi: ProductApiService,
    private warehouseApi: WarehouseApiService,
    private userApi: UserApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.productApi.getAll().subscribe({ next: rows => (this.products = rows), error: () => (this.products = []) });
    this.warehouseApi.getAll().subscribe({ next: rows => (this.warehouses = rows), error: () => (this.warehouses = []) });
    this.userApi.getAll().subscribe({ next: rows => (this.users = rows), error: () => (this.users = []) });
  }

  private emptyForm() {
    return { productId: null, warehouseId: null, quantity: null, orderDate: new Date(), ownerId: null };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.orders = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load production orders.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(order: ProductionOrderDto) {
    this.editing = order;
    this.form = {
      productId: order.productId, warehouseId: order.warehouseId, quantity: order.quantity,
      orderDate: new Date(order.orderDate), ownerId: order.ownerId
    };
    this.showDialog = true;
  }

  save() {
    if ((!this.editing && !this.form.productId) || !this.form.warehouseId || !this.form.quantity || !this.form.ownerId) {
      this.notify.warn('Finished good, warehouse, quantity, and owner are required.');
      return;
    }
    this.saving = true;
    const orderDate = (this.form.orderDate ?? new Date()).toISOString();
    const req$ = this.editing
      ? this.api.update(this.editing.id, {
          warehouseId: this.form.warehouseId,
          quantity: this.form.quantity,
          orderDate,
          ownerId: this.form.ownerId
        } as UpdateProductionOrderDto)
      : this.api.create({
          productId: this.form.productId,
          warehouseId: this.form.warehouseId,
          quantity: this.form.quantity,
          orderDate,
          ownerId: this.form.ownerId
        } as CreateProductionOrderDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Production order ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save production order.');
      }
    });
  }

  delete(order: ProductionOrderDto) {
    this.confirm.confirm({
      message: `Delete production order "${order.moNumber}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(order.id).subscribe({
          next: () => {
            this.notify.success('Production order deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete production order.')
        });
      }
    });
  }

  complete(order: ProductionOrderDto) {
    this.api.complete(order.id).subscribe({
      next: () => {
        this.notify.success('Production order completed.');
        this.load();
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to complete production order.')
    });
  }

  cancel(order: ProductionOrderDto) {
    this.confirm.confirm({
      message: `Cancel production order "${order.moNumber}"?`,
      header: 'Confirm Cancel',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.cancel(order.id).subscribe({
          next: () => {
            this.notify.success('Production order cancelled.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to cancel production order.')
        });
      }
    });
  }

  statusSeverity(status: string): 'success' | 'danger' | 'info' | 'warn' {
    if (status === 'Completed') return 'success';
    if (status === 'Cancelled') return 'danger';
    return 'info';
  }
}

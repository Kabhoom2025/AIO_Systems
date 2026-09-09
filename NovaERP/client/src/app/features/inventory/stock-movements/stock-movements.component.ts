import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  StockMovementApiService, StockMovementDto, CreateStockMovementDto
} from '../../../core/stock-movement-api.service';
import { ProductApiService, ProductDto } from '../../../core/product-api.service';
import { WarehouseApiService, WarehouseDto } from '../../../core/warehouse-api.service';

const MOVEMENT_TYPES = ['Receipt', 'Issue', 'Adjustment'];

@Component({
  selector: 'app-stock-movements',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, DropdownModule, TagModule,
    ToastModule, HasPermissionDirective
  ],
  templateUrl: './stock-movements.component.html',
  styleUrl: './stock-movements.component.scss'
})
export class StockMovementsComponent implements OnInit {
  movements: StockMovementDto[] = [];
  products: ProductDto[] = [];
  warehouses: WarehouseDto[] = [];
  movementTypes = MOVEMENT_TYPES;
  loading = false;

  showDialog = false;
  saving = false;
  form: { productId: number | null; warehouseId: number | null; movementType: string; quantity: number | null; notes: string | null } = this.emptyForm();

  constructor(
    private api: StockMovementApiService,
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
    return { productId: null, warehouseId: null, movementType: 'Adjustment', quantity: null, notes: null };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.movements = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load stock movements.');
      }
    });
  }

  openNew() {
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  save() {
    if (!this.form.productId || this.form.quantity == null || this.form.quantity === 0) {
      this.notify.warn('Product and a non-zero quantity are required.');
      return;
    }
    this.saving = true;
    this.api.create({
      productId: this.form.productId,
      warehouseId: this.form.warehouseId,
      movementType: this.form.movementType,
      quantity: this.form.quantity,
      notes: this.form.notes
    } as CreateStockMovementDto).subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success('Stock movement recorded.');
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to record stock movement.');
      }
    });
  }

  typeSeverity(type: string): 'success' | 'danger' | 'info' {
    if (type === 'Receipt') return 'success';
    if (type === 'Issue') return 'danger';
    return 'info';
  }
}

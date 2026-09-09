import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  ProductApiService, ProductDto, CreateProductDto, UpdateProductDto
} from '../../../core/product-api.service';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, CheckboxModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './products.component.html',
  styleUrl: './products.component.scss'
})
export class ProductsComponent implements OnInit {
  products: ProductDto[] = [];
  loading = false;

  showDialog = false;
  editing: ProductDto | null = null;
  saving = false;

  form: { sku: string; name: string; description: string | null; unitOfMeasure: string; unitCost: number | null; weightKg: number | null; isActive: boolean } = this.emptyForm();

  constructor(
    private api: ProductApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  private emptyForm() {
    return { sku: '', name: '', description: null, unitOfMeasure: 'EA', unitCost: null, weightKg: null, isActive: true };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.products = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load products.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(product: ProductDto) {
    this.editing = product;
    this.form = {
      sku: product.sku, name: product.name, description: product.description,
      unitOfMeasure: product.unitOfMeasure, unitCost: product.unitCost,
      weightKg: product.weightKg, isActive: product.isActive
    };
    this.showDialog = true;
  }

  save() {
    if (!this.form.sku || !this.form.name) {
      this.notify.warn('SKU and name are required.');
      return;
    }
    this.saving = true;
    const payload = {
      name: this.form.name,
      description: this.form.description,
      unitOfMeasure: this.form.unitOfMeasure,
      unitCost: this.form.unitCost ?? 0,
      weightKg: this.form.weightKg,
      isActive: this.form.isActive
    };
    const req$ = this.editing
      ? this.api.update(this.editing.id, payload as UpdateProductDto)
      : this.api.create({ ...payload, sku: this.form.sku } as CreateProductDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Product ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save product.');
      }
    });
  }

  delete(product: ProductDto) {
    this.confirm.confirm({
      message: `Delete product "${product.sku}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(product.id).subscribe({
          next: () => {
            this.notify.success('Product deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete product.')
        });
      }
    });
  }
}

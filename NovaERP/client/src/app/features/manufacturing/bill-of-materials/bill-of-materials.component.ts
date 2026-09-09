import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  BillOfMaterialApiService, BillOfMaterialDto, CreateBillOfMaterialDto, UpdateBillOfMaterialDto, CreateBomComponentDto
} from '../../../core/bill-of-material-api.service';
import { ProductApiService, ProductDto } from '../../../core/product-api.service';

interface ComponentForm {
  componentProductId: number | null;
  quantity: number | null;
  displayOrder: number;
}

@Component({
  selector: 'app-bill-of-materials',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputNumberModule, DropdownModule, CheckboxModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './bill-of-materials.component.html',
  styleUrl: './bill-of-materials.component.scss'
})
export class BillOfMaterialsComponent implements OnInit {
  boms: BillOfMaterialDto[] = [];
  products: ProductDto[] = [];
  loading = false;

  showDialog = false;
  editing: BillOfMaterialDto | null = null;
  saving = false;

  form: { productId: number | null; isActive: boolean } = this.emptyForm();
  components: ComponentForm[] = [];

  constructor(
    private api: BillOfMaterialApiService,
    private productApi: ProductApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.productApi.getAll().subscribe({ next: rows => (this.products = rows), error: () => (this.products = []) });
  }

  private emptyForm() {
    return { productId: null, isActive: true };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.boms = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load bills of materials.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.components = [];
    this.showDialog = true;
  }

  openEdit(bom: BillOfMaterialDto) {
    this.editing = bom;
    this.form = { productId: bom.productId, isActive: bom.isActive };
    this.components = bom.components
      .slice()
      .sort((a, b) => a.displayOrder - b.displayOrder)
      .map(c => ({ componentProductId: c.componentProductId, quantity: c.quantity, displayOrder: c.displayOrder }));
    this.showDialog = true;
  }

  addComponent() {
    const nextOrder = this.components.length ? Math.max(...this.components.map(c => c.displayOrder)) + 1 : 1;
    this.components.push({ componentProductId: null, quantity: null, displayOrder: nextOrder });
  }

  removeComponent(index: number) {
    this.components.splice(index, 1);
  }

  save() {
    if (!this.editing && !this.form.productId) {
      this.notify.warn('A finished-good product is required.');
      return;
    }
    if (!this.components.length || this.components.some(c => !c.componentProductId || c.quantity == null || c.quantity <= 0)) {
      this.notify.warn('Every component needs a product and a positive quantity.');
      return;
    }
    this.saving = true;
    const componentDtos: CreateBomComponentDto[] = this.components.map(c => ({
      componentProductId: c.componentProductId!,
      quantity: c.quantity!,
      displayOrder: c.displayOrder
    }));
    const req$ = this.editing
      ? this.api.update(this.editing.id, {
          isActive: this.form.isActive,
          components: componentDtos
        } as UpdateBillOfMaterialDto)
      : this.api.create({
          productId: this.form.productId,
          isActive: this.form.isActive,
          components: componentDtos
        } as CreateBillOfMaterialDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Bill of materials ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save bill of materials.');
      }
    });
  }

  delete(bom: BillOfMaterialDto) {
    this.confirm.confirm({
      message: `Delete bill of materials for "${bom.productName}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(bom.id).subscribe({
          next: () => {
            this.notify.success('Bill of materials deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete bill of materials.')
        });
      }
    });
  }
}

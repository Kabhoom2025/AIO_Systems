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
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import {
  TaxCodeApiService, TaxCodeDto,
  CreateTaxCodeDto, UpdateTaxCodeDto, CreateTaxComponentDto
} from '../../core/tax-code-api.service';

interface ComponentForm {
  name: string;
  ratePercent: number | null;
  displayOrder: number;
}

@Component({
  selector: 'app-tax-codes',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, CheckboxModule, TagModule,
    ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './tax-codes.component.html',
  styleUrl: './tax-codes.component.scss'
})
export class TaxCodesComponent implements OnInit {
  taxCodes: TaxCodeDto[] = [];
  loading = false;

  showDialog = false;
  editing: TaxCodeDto | null = null;
  saving = false;

  form: { code: string; name: string; isActive: boolean } = this.emptyForm();
  components: ComponentForm[] = [];

  constructor(
    private api: TaxCodeApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  private emptyForm() {
    return { code: '', name: '', isActive: true };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.taxCodes = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load tax codes.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.components = [];
    this.showDialog = true;
  }

  openEdit(taxCode: TaxCodeDto) {
    this.editing = taxCode;
    this.form = { code: taxCode.code, name: taxCode.name, isActive: taxCode.isActive };
    this.components = taxCode.components
      .slice()
      .sort((a, b) => a.displayOrder - b.displayOrder)
      .map(c => ({ name: c.name, ratePercent: c.ratePercent, displayOrder: c.displayOrder }));
    this.showDialog = true;
  }

  addComponent() {
    const nextOrder = this.components.length ? Math.max(...this.components.map(c => c.displayOrder)) + 1 : 1;
    this.components.push({ name: '', ratePercent: null, displayOrder: nextOrder });
  }

  removeComponent(index: number) {
    this.components.splice(index, 1);
  }

  totalRate(): number {
    return this.components.reduce((sum, c) => sum + (c.ratePercent ?? 0), 0);
  }

  save() {
    if (!this.form.code || !this.form.name) {
      this.notify.warn('Code and name are required.');
      return;
    }
    if (!this.components.length) {
      this.notify.warn('Add at least one rate component.');
      return;
    }
    if (this.components.some(c => !c.name || c.ratePercent == null)) {
      this.notify.warn('Every component needs a name and rate.');
      return;
    }
    this.saving = true;
    const componentDtos: CreateTaxComponentDto[] = this.components.map(c => ({
      name: c.name,
      ratePercent: c.ratePercent!,
      displayOrder: c.displayOrder
    }));
    const req$ = this.editing
      ? this.api.update(this.editing.id, {
          name: this.form.name,
          isActive: this.form.isActive,
          components: componentDtos
        } as UpdateTaxCodeDto)
      : this.api.create({
          code: this.form.code,
          name: this.form.name,
          isActive: this.form.isActive,
          components: componentDtos
        } as CreateTaxCodeDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Tax code ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save tax code.');
      }
    });
  }

  delete(taxCode: TaxCodeDto) {
    this.confirm.confirm({
      message: `Delete tax code "${taxCode.code}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(taxCode.id).subscribe({
          next: () => {
            this.notify.success('Tax code deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete tax code.')
        });
      }
    });
  }
}

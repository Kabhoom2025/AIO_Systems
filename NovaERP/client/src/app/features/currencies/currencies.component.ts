import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import { CurrencyApiService, CurrencyDto, CreateCurrencyDto, UpdateCurrencyDto } from '../../core/currency-api.service';

@Component({
  selector: 'app-currencies',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, InputNumberModule, TagModule, ToastModule, HasPermissionDirective
  ],
  templateUrl: './currencies.component.html',
  styleUrl: './currencies.component.scss'
})
export class CurrenciesComponent implements OnInit {
  currencies: CurrencyDto[] = [];
  loading = false;

  showDialog = false;
  editing: CurrencyDto | null = null;
  form: CreateCurrencyDto & { isActive?: boolean } = this.emptyForm();
  saving = false;

  constructor(private api: CurrencyApiService, private notify: NotificationService) {}

  ngOnInit(): void {
    this.load();
  }

  private emptyForm(): CreateCurrencyDto & { isActive?: boolean } {
    return { code: '', name: '', symbol: '', decimalPlaces: 2 };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.currencies = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load currencies.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(currency: CurrencyDto) {
    this.editing = currency;
    this.form = {
      code: currency.code,
      name: currency.name,
      symbol: currency.symbol,
      decimalPlaces: currency.decimalPlaces,
      isActive: currency.isActive
    };
    this.showDialog = true;
  }

  save() {
    if (!this.form.code.trim() || !this.form.name.trim()) {
      this.notify.warn('Code and name are required.');
      return;
    }
    this.saving = true;
    const req$ = this.editing
      ? this.api.update(this.editing.code, {
          name: this.form.name,
          symbol: this.form.symbol,
          decimalPlaces: this.form.decimalPlaces,
          isActive: this.form.isActive ?? true
        } as UpdateCurrencyDto)
      : this.api.create(this.form);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Currency ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save currency.');
      }
    });
  }
}

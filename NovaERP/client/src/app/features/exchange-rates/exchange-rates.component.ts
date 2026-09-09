import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import {
  ExchangeRateApiService, ExchangeRateDto, CreateExchangeRateDto, UpdateExchangeRateDto
} from '../../core/exchange-rate-api.service';
import { CurrencyApiService, CurrencyDto } from '../../core/currency-api.service';

@Component({
  selector: 'app-exchange-rates',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputNumberModule, DropdownModule, CalendarModule, ToastModule,
    ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './exchange-rates.component.html',
  styleUrl: './exchange-rates.component.scss'
})
export class ExchangeRatesComponent implements OnInit {
  rates: ExchangeRateDto[] = [];
  currencies: CurrencyDto[] = [];
  loading = false;

  showDialog = false;
  editing: ExchangeRateDto | null = null;
  form: { fromCurrencyCode: string | null; toCurrencyCode: string | null; rate: number | null; effectiveDate: Date | null } = this.emptyForm();
  saving = false;

  constructor(
    private api: ExchangeRateApiService,
    private currencyApi: CurrencyApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.currencyApi.getAll().subscribe({ next: rows => (this.currencies = rows), error: () => (this.currencies = []) });
  }

  private emptyForm() {
    return { fromCurrencyCode: null, toCurrencyCode: null, rate: null, effectiveDate: new Date() };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.rates = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load exchange rates.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(rate: ExchangeRateDto) {
    this.editing = rate;
    this.form = {
      fromCurrencyCode: rate.fromCurrencyCode,
      toCurrencyCode: rate.toCurrencyCode,
      rate: rate.rate,
      effectiveDate: new Date(rate.effectiveDate)
    };
    this.showDialog = true;
  }

  save() {
    if (!this.form.fromCurrencyCode || !this.form.toCurrencyCode || this.form.rate == null || !this.form.effectiveDate) {
      this.notify.warn('From/To currency, rate and effective date are required.');
      return;
    }
    this.saving = true;
    const effectiveDate = this.form.effectiveDate.toISOString();
    const req$ = this.editing
      ? this.api.update(this.editing.id, { rate: this.form.rate, effectiveDate } as UpdateExchangeRateDto)
      : this.api.create({
          fromCurrencyCode: this.form.fromCurrencyCode,
          toCurrencyCode: this.form.toCurrencyCode,
          rate: this.form.rate,
          effectiveDate
        } as CreateExchangeRateDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Exchange rate ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save exchange rate.');
      }
    });
  }

  delete(rate: ExchangeRateDto) {
    this.confirm.confirm({
      message: `Delete exchange rate ${rate.fromCurrencyCode} -> ${rate.toCurrencyCode}?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(rate.id).subscribe({
          next: () => {
            this.notify.success('Exchange rate deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete exchange rate.')
        });
      }
    });
  }
}

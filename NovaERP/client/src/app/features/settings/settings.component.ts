import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import { SettingsApiService, OrganizationSettingsDto, UpdateOrganizationSettingsDto } from '../../core/settings-api.service';
import { LanguageApiService, LanguageDto } from '../../core/language-api.service';
import { CurrencyApiService, CurrencyDto } from '../../core/currency-api.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ButtonModule, InputTextModule, InputNumberModule,
    DropdownModule, CheckboxModule, ToastModule, HasPermissionDirective
  ],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent implements OnInit {
  loading = false;
  saving = false;

  languages: LanguageDto[] = [];
  currencies: CurrencyDto[] = [];

  fiscalMonths = Array.from({ length: 12 }, (_, i) => ({ label: this.monthName(i + 1), value: i + 1 }));

  settingsId: number | null = null;
  form: UpdateOrganizationSettingsDto = this.emptyForm();

  constructor(
    private api: SettingsApiService,
    private languageApi: LanguageApiService,
    private currencyApi: CurrencyApiService,
    private notify: NotificationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.languageApi.getAll().subscribe({ next: rows => (this.languages = rows), error: () => (this.languages = []) });
    this.currencyApi.getAll().subscribe({ next: rows => (this.currencies = rows), error: () => (this.currencies = []) });
  }

  private monthName(month: number): string {
    return new Date(2000, month - 1, 1).toLocaleString('en-US', { month: 'long' });
  }

  private emptyForm(): UpdateOrganizationSettingsDto {
    return {
      defaultLanguageCode: 'en',
      defaultCurrencyCode: 'INR',
      defaultTimezone: 'Asia/Kolkata',
      dateFormat: 'dd/MM/yyyy',
      timeFormat: 'HH:mm',
      fiscalYearStartMonth: 4,
      invoiceNumberPrefix: 'INV',
      taxInclusivePricing: false
    };
  }

  load() {
    this.loading = true;
    this.api.get().subscribe({
      next: (settings: OrganizationSettingsDto) => {
        this.settingsId = settings.id;
        this.form = {
          defaultLanguageCode: settings.defaultLanguageCode,
          defaultCurrencyCode: settings.defaultCurrencyCode,
          defaultTimezone: settings.defaultTimezone,
          dateFormat: settings.dateFormat,
          timeFormat: settings.timeFormat,
          fiscalYearStartMonth: settings.fiscalYearStartMonth,
          invoiceNumberPrefix: settings.invoiceNumberPrefix,
          taxInclusivePricing: settings.taxInclusivePricing
        };
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load settings.');
      }
    });
  }

  save() {
    this.saving = true;
    this.api.update(this.form).subscribe({
      next: () => {
        this.saving = false;
        this.notify.success('Settings updated.');
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save settings.');
      }
    });
  }
}

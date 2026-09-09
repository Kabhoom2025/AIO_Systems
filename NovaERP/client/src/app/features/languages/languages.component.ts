import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { CheckboxModule } from 'primeng/checkbox';
import { HasPermissionDirective } from '../../core/permission.directive';
import { NotificationService } from '../../core/notification.service';
import { LanguageApiService, LanguageDto } from '../../core/language-api.service';
import {
  OrganizationLanguageApiService, OrganizationLanguageDto
} from '../../core/organization-language-api.service';

interface LanguageSelection {
  code: string;
  name: string;
  selected: boolean;
}

@Component({
  selector: 'app-languages',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, ButtonModule, TagModule,
    ToastModule, CheckboxModule, HasPermissionDirective
  ],
  templateUrl: './languages.component.html',
  styleUrl: './languages.component.scss'
})
export class LanguagesComponent implements OnInit {
  languages: LanguageDto[] = [];
  loadingLanguages = false;

  orgLanguages: OrganizationLanguageDto[] = [];
  loadingOrgLanguages = false;
  saving = false;

  selections: LanguageSelection[] = [];
  defaultLanguageCode: string | null = null;

  constructor(
    private languageApi: LanguageApiService,
    private orgLanguageApi: OrganizationLanguageApiService,
    private notify: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadLanguages();
    this.loadOrgLanguages();
  }

  loadLanguages() {
    this.loadingLanguages = true;
    this.languageApi.getAll().subscribe({
      next: rows => { this.languages = rows; this.loadingLanguages = false; this.syncSelections(); },
      error: err => {
        this.loadingLanguages = false;
        this.notify.error(err.error?.message ?? 'Failed to load languages.');
      }
    });
  }

  loadOrgLanguages() {
    this.loadingOrgLanguages = true;
    this.orgLanguageApi.getAll().subscribe({
      next: rows => {
        this.orgLanguages = rows;
        this.defaultLanguageCode = rows.find(r => r.isDefault)?.languageCode ?? null;
        this.loadingOrgLanguages = false;
        this.syncSelections();
      },
      error: err => {
        this.loadingOrgLanguages = false;
        this.notify.error(err.error?.message ?? 'Failed to load organization languages.');
      }
    });
  }

  private syncSelections() {
    if (this.languages.length === 0) return;
    const selectedCodes = new Set(this.orgLanguages.map(l => l.languageCode));
    this.selections = this.languages.map(l => ({
      code: l.code,
      name: l.name,
      selected: selectedCodes.has(l.code)
    }));
  }

  onSelectionChange(selection: LanguageSelection) {
    if (!selection.selected && selection.code === this.defaultLanguageCode) {
      // Can't deselect the default language without choosing another first.
      selection.selected = true;
      this.notify.warn('Choose a different default language before removing this one.');
    }
  }

  setDefault(code: string) {
    this.defaultLanguageCode = code;
    const selection = this.selections.find(s => s.code === code);
    if (selection) selection.selected = true;
  }

  saveOrgLanguages() {
    const languageCodes = this.selections.filter(s => s.selected).map(s => s.code);
    if (languageCodes.length === 0) {
      this.notify.warn('Select at least one supported language.');
      return;
    }
    if (!this.defaultLanguageCode || !languageCodes.includes(this.defaultLanguageCode)) {
      this.notify.warn('Pick a default language among the selected languages.');
      return;
    }
    this.saving = true;
    this.orgLanguageApi.update({ languageCodes, defaultLanguageCode: this.defaultLanguageCode }).subscribe({
      next: () => {
        this.saving = false;
        this.notify.success('Supported languages updated.');
        this.loadOrgLanguages();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to update supported languages.');
      }
    });
  }
}

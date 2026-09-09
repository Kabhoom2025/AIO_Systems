import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { HasPermissionDirective } from '../../../core/permission.directive';
import { NotificationService } from '../../../core/notification.service';
import {
  AssetCategoryApiService, AssetCategoryDto, CreateAssetCategoryDto, UpdateAssetCategoryDto
} from '../../../core/asset-category-api.service';

@Component({
  selector: 'app-asset-categories',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, DialogModule, ButtonModule,
    InputTextModule, CheckboxModule, TagModule, ToastModule, ConfirmDialogModule, HasPermissionDirective
  ],
  providers: [ConfirmationService],
  templateUrl: './asset-categories.component.html',
  styleUrl: './asset-categories.component.scss'
})
export class AssetCategoriesComponent implements OnInit {
  categories: AssetCategoryDto[] = [];
  loading = false;

  showDialog = false;
  editing: AssetCategoryDto | null = null;
  saving = false;

  form: { name: string; code: string; isActive: boolean } = this.emptyForm();

  constructor(
    private api: AssetCategoryApiService,
    private notify: NotificationService,
    private confirm: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  private emptyForm() {
    return { name: '', code: '', isActive: true };
  }

  load() {
    this.loading = true;
    this.api.getAll().subscribe({
      next: rows => { this.categories = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load asset categories.');
      }
    });
  }

  openNew() {
    this.editing = null;
    this.form = this.emptyForm();
    this.showDialog = true;
  }

  openEdit(category: AssetCategoryDto) {
    this.editing = category;
    this.form = { name: category.name, code: category.code, isActive: category.isActive };
    this.showDialog = true;
  }

  save() {
    if (!this.form.name || (!this.editing && !this.form.code)) {
      this.notify.warn('Name and code are required.');
      return;
    }
    this.saving = true;
    const payload = { name: this.form.name, isActive: this.form.isActive };
    const req$ = this.editing
      ? this.api.update(this.editing.id, payload as UpdateAssetCategoryDto)
      : this.api.create({ ...payload, code: this.form.code } as CreateAssetCategoryDto);
    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showDialog = false;
        this.notify.success(`Asset category ${this.editing ? 'updated' : 'created'}.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Failed to save asset category.');
      }
    });
  }

  delete(category: AssetCategoryDto) {
    this.confirm.confirm({
      message: `Delete asset category "${category.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.delete(category.id).subscribe({
          next: () => {
            this.notify.success('Asset category deleted.');
            this.load();
          },
          error: err => this.notify.error(err.error?.message ?? 'Failed to delete asset category.')
        });
      }
    });
  }
}
